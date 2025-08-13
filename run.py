#!/usr/bin/env python3
"""
MAFFENG - Launcher principal
Script de inicialização centralizado para aplicação C# ASP.NET Core
"""
import subprocess
import sys
import os

if __name__ == "__main__":
    print("MAFFENG - Gerador de Relatórios Fotográficos")
    print("Iniciando aplicação C# ASP.NET Core na porta 5000...")
    
    # Executa a aplicação C# diretamente
    try:
        os.execvp('dotnet', ['dotnet', 'run', '--urls=http://0.0.0.0:5000'])
    except FileNotFoundError:
        print("Erro: .NET SDK não encontrado")
        sys.exit(1)
    except Exception as e:
        print(f"Erro ao iniciar aplicação: {e}")
        sys.exit(1)