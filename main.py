#!/usr/bin/env python3
"""
Entry point that starts the C# ASP.NET Core MAFFENG application directly.
This script is called by Gunicorn but immediately hands control to the C# app.
"""
import os
import sys
import signal

def signal_handler(sig, frame):
    """Handle shutdown signals"""
    print("Shutting down...")
    sys.exit(0)

# Set up signal handling
signal.signal(signal.SIGINT, signal_handler)
signal.signal(signal.SIGTERM, signal_handler)

def main():
    """Launch the C# application and replace this process"""
    print("MAFFENG Application Launcher - Starting C# ASP.NET Core application...")
    
    try:
        # Change to workspace directory
        os.chdir('/home/runner/workspace')
        
        # Set up environment
        env = os.environ.copy()
        env['ASPNETCORE_ENVIRONMENT'] = 'Production'
        env['ASPNETCORE_URLS'] = 'http://0.0.0.0:5000'
        
        # Replace this Python process with the C# application
        print("Executing dotnet run...")
        os.execvpe('dotnet', ['dotnet', 'run', '--urls=http://0.0.0.0:5000'], env)
        
    except Exception as e:
        print(f"Failed to start C# application: {e}")
        print("Attempting alternative startup method...")
        
        # Fallback method
        import subprocess
        env = os.environ.copy()
        env['ASPNETCORE_ENVIRONMENT'] = 'Production'
        subprocess.call([
            'dotnet', 'run', '--urls=http://0.0.0.0:5000'
        ], cwd='/home/runner/workspace', env=env)

# Create a minimal Flask app for Gunicorn compatibility
from flask import Flask

app = Flask(__name__)

@app.route('/')
def index():
    return "Starting MAFFENG Application...", 503

# When this module is imported by Gunicorn, start the C# app
if __name__ != '__main__':
    # This runs when Gunicorn imports the module
    import threading
    import time
    
    def delayed_start():
        time.sleep(1)  # Give Gunicorn a moment to set up
        main()
    
    threading.Thread(target=delayed_start, daemon=False).start()

if __name__ == '__main__':
    # Direct execution
    main()