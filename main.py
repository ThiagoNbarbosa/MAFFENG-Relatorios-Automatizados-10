"""
MAFFENG - Gerador de Relatórios Fotográficos
Executa diretamente a aplicação C# ASP.NET Core
"""
import subprocess
import sys
import os

def main():
    """Executa a aplicação MAFFENG C# diretamente"""
    print("🚀 Iniciando MAFFENG - Gerador de Relatórios Fotográficos")
    print("📍 Aplicação C# ASP.NET Core")
    print("🌐 Porta: 5000")
    print("-" * 50)
    
    try:
        # Executa a aplicação C# diretamente
        subprocess.run([
            'dotnet', 'run', 
            '--urls=http://0.0.0.0:5000'
        ], check=True)
    except subprocess.CalledProcessError as e:
        print(f"❌ Erro ao executar aplicação: {e}")
        sys.exit(1)
    except KeyboardInterrupt:
        print("🛑 Aplicação encerrada")
        sys.exit(0)

# Compatibilidade para quando executado pelo gunicorn (não deve ser usado)
from flask import Flask
app = Flask(__name__)

@app.route('/')
def redirect_notice():
    return '''
    <h1>MAFFENG - Configuração Incorreta</h1>
    <p>Esta aplicação deve executar diretamente via dotnet run, não via gunicorn.</p>
    <p>Use: <code>dotnet run --urls=http://0.0.0.0:5000</code></p>
    '''

if __name__ == "__main__":
    main()