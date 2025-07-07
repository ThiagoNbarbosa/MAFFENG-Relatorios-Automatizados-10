
#!/usr/bin/env python3
"""
Script para processar e formatar documento Word anexado
Aplica as melhorias solicitadas:
- Formatação Calibri 11pt para placeholders
- Espaçamento após imagens
- Organização por colunas baseada no tamanho
"""

import os
import shutil
from docx_formatter import processar_arquivo_anexado

def main():
    # Localizar arquivo anexado
    attached_files = [
        "attached_assets/RELATÓRIO FOTOGRÁFICO - -_PRIMEIRO_TESTE_- - LEVANTAMENTO PREVENTIVO_1751864925704.docx",
        "attached_assets/RELATÓRIO FOTOGRÁFICO - -_PRIMEIRO_TESTE_- - LEVANTAMENTO PREVENTIVO_1751864135810.docx"
    ]
    
    arquivo_encontrado = None
    for arquivo in attached_files:
        if os.path.exists(arquivo):
            arquivo_encontrado = arquivo
            break
    
    if not arquivo_encontrado:
        print("❌ Nenhum arquivo .docx encontrado na pasta attached_assets/")
        return False
    
    print(f"📄 Processando arquivo: {arquivo_encontrado}")
    
    # Garantir que pasta output existe
    os.makedirs("output", exist_ok=True)
    
    # Processar arquivo
    sucesso = processar_arquivo_anexado(arquivo_encontrado)
    
    if sucesso:
        print("✅ Documento processado com sucesso!")
        print("📁 Arquivo formatado salvo na pasta 'output/'")
        
        # Listar arquivos na pasta output
        output_files = [f for f in os.listdir("output") if f.endswith("_FORMATADO.docx")]
        if output_files:
            print(f"📋 Arquivo gerado: {output_files[-1]}")
    else:
        print("❌ Erro ao processar documento")
    
    return sucesso

if __name__ == "__main__":
    main()
