"""
MAFFENG - Launcher Flask para aplicação C# ASP.NET Core
"""
import subprocess
import os
from threading import Thread
from flask import Flask

# Inicializa aplicação Flask minimalista
app = Flask(__name__)

# Processo C# global
csharp_process = None

def start_csharp_app():
    """Inicia a aplicação C# ASP.NET Core"""
    global csharp_process
    try:
        print("Iniciando aplicação C# ASP.NET Core...")
        csharp_process = subprocess.Popen([
            'dotnet', 'run', 
            '--urls=http://0.0.0.0:5000'
        ])
        csharp_process.wait()
    except Exception as e:
        print(f"Erro ao executar aplicação C#: {e}")

@app.route('/')
def index():
    return "MAFFENG - Aplicação sendo redirecionada para C# ASP.NET Core"

# Inicia a aplicação C# quando importado
if __name__ != '__main__':
    # Executado pelo gunicorn
    csharp_thread = Thread(target=start_csharp_app, daemon=True)
    csharp_thread.start()

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)