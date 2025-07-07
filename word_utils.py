import os
import zipfile
import tempfile
import shutil
from datetime import datetime
from docx import Document
from docx.shared import Cm, Pt
from docx.enum.text import WD_PARAGRAPH_ALIGNMENT, WD_BREAK
from PIL import Image, UnidentifiedImageError

# Folder processing order as specified
ORDEM_PASTAS = [
    "- Área externa", 
    "- Área interna", 
    "- Segundo piso",
    "- Detalhes",
    "- Vista ampla"
]

# Folders that should use normal text with bold instead of headings
PASTAS_TEXTO_NORMAL = ["- Detalhes", "- Vista ampla"]

def processar_zip(zip_path, dados_formulario):
    """
    Extract ZIP file and organize folder structure
    Returns structured content list for Word document insertion
    """
    print(f"Processing ZIP file: {zip_path}")
    
    # Create temporary directory for extraction
    with tempfile.TemporaryDirectory() as temp_dir:
        # Extract ZIP file
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            zip_ref.extractall(temp_dir)
        
        # Find the root folder (should be the only folder in temp_dir)
        extracted_items = os.listdir(temp_dir)
        if len(extracted_items) == 1 and os.path.isdir(os.path.join(temp_dir, extracted_items[0])):
            pasta_raiz = os.path.join(temp_dir, extracted_items[0])
        else:
            pasta_raiz = temp_dir
        
        # Process folder structure
        conteudo = []
        
        # Walk through directory structure
        for root, dirs, files in os.walk(pasta_raiz):
            # Sort directories according to specified order
            if root == pasta_raiz:
                dirs.sort(key=lambda x: (ORDEM_PASTAS.index(x) if x in ORDEM_PASTAS else len(ORDEM_PASTAS), x))
            
            # Calculate relative path and hierarchy level
            rel_path = os.path.relpath(root, pasta_raiz)
            if rel_path == '.':
                continue  # Skip root directory
            
            path_parts = rel_path.split(os.sep)
            nivel = len(path_parts) - 1
            
            # Add folder title based on hierarchy level
            folder_name = path_parts[-1]
            if nivel == 0:
                conteudo.append(folder_name)
            elif nivel == 1:
                conteudo.append(f"»{folder_name}")
            elif nivel == 2:
                conteudo.append(f"»»{folder_name}")
            else:
                conteudo.append(f"»»»{folder_name}")
            
            # Process images in current folder
            arquivos_imagens = [
                os.path.join(root, file)
                for file in files
                if file.lower().endswith(('.png', '.jpg', '.jpeg'))
            ]
            
            # Sort images by creation time
            arquivos_imagens.sort(key=lambda x: os.path.getctime(x) if os.path.exists(x) else 0)
            
            # Add images to content
            for imagem_path in arquivos_imagens:
                # Copy image to temporary location for processing
                temp_image_path = os.path.join(tempfile.gettempdir(), f"temp_img_{os.path.basename(imagem_path)}")
                shutil.copy2(imagem_path, temp_image_path)
                conteudo.append({"imagem": temp_image_path})
            
            # Add page break after each folder section
            if arquivos_imagens:  # Only add page break if there were images
                conteudo.append({"quebra_pagina": True})
        
        return conteudo

def substituir_placeholders(doc, dados_formulario, placeholders):
    """
    Replace placeholders in Word document with form data and apply specific formatting
    """
    print("Replacing placeholders in document...")
    
    # Create data mapping for placeholders
    placeholder_data = {}
    for placeholder, form_field in placeholders.items():
        if form_field in dados_formulario:
            placeholder_data[placeholder] = dados_formulario[form_field]
        elif form_field == 'Ygor Augusto Fernandes':
            placeholder_data[placeholder] = 'Ygor Augusto Fernandes'
        elif form_field == 'MAFFENG - Engenharia e Manutenção Profissional':
            placeholder_data[placeholder] = 'MAFFENG - Engenharia e Manutenção Profissional'
    
    # Replace placeholders in paragraphs with specific formatting
    for paragraph in doc.paragraphs:
        for placeholder, value in placeholder_data.items():
            if placeholder in paragraph.text:
                # Clear the paragraph and rebuild with proper formatting
                paragraph.clear()
                run = paragraph.add_run(str(value))
                aplicar_estilo_placeholder(run, placeholder)
    
    # Replace placeholders in tables with specific formatting
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                for paragraph in cell.paragraphs:
                    for placeholder, value in placeholder_data.items():
                        if placeholder in paragraph.text:
                            # Clear the paragraph and rebuild with proper formatting
                            paragraph.clear()
                            run = paragraph.add_run(str(value))
                            aplicar_estilo_placeholder(run, placeholder)
    
    print(f"Replaced {len(placeholder_data)} placeholders")

def aplicar_estilo(run, tamanho, negrito=False, fonte="Arial"):
    """Apply font styling to text run"""
    run.font.name = fonte
    run.font.size = Pt(tamanho)
    run.bold = negrito

def aplicar_estilo_placeholder(run, placeholder):
    """Apply specific font styling based on placeholder type"""
    # Arial PT 11 placeholders
    arial_placeholders = [
        '{{endereco_dependencia}}',
        '{{responsavel_dependencia}}',
        '{{responsavel_tecnico}}'
    ]
    
    # Calibri PT 11 placeholders  
    calibri_placeholders = [
        '{{ordem_servico}}',
        '{{data_elaboracao}}',
        '{{data_atendimento}}',
        '{{tipo_atendimento}}',
        '{{prefixo_sb}}',
        '{{nome_ag}}',
        '{{uf}}'
    ]
    
    if placeholder in arial_placeholders:
        run.font.name = "Arial"
        run.font.size = Pt(11)
    elif placeholder in calibri_placeholders:
        run.font.name = "Calibri"
        run.font.size = Pt(11)
    else:
        # Default formatting
        run.font.name = "Calibri"
        run.font.size = Pt(11)

def inserir_conteudo_word(modelo_path, conteudo, placeholders, dados_formulario, output_path):
    """
    Insert content into Word template and generate final document
    Returns number of images inserted
    """
    print(f"Loading Word template: {modelo_path}")
    
    # Load Word document
    doc = Document(modelo_path)
    contador_imagens = 0
    
    # Replace placeholders first
    substituir_placeholders(doc, dados_formulario, placeholders)
    
    # Find insertion point
    paragrafo_insercao_index = None
    for i, paragrafo in enumerate(doc.paragraphs):
        if "{{start_here}}" in paragrafo.text:
            paragrafo_insercao_index = i
            # Clear the start_here marker
            paragrafo.text = paragrafo.text.replace("{{start_here}}", "")
            break
    
    if paragrafo_insercao_index is None:
        print("Warning: '{{start_here}}' marker not found in template")
        # Use last paragraph as insertion point
        paragrafo_insercao_index = len(doc.paragraphs) - 1
    
    # Insert content in reverse order to maintain proper positioning
    conteudo_invertido = list(reversed(conteudo))
    
    for item in conteudo_invertido:
        if isinstance(item, str):
            # Process folder titles
            titulo = item.replace("»", "").strip() + ":"
            nivel = item.count("»")
            
            # Insert new paragraph
            p = doc.paragraphs[paragrafo_insercao_index].insert_paragraph_before('')
            run = p.add_run(titulo)
            
            # Apply styling based on folder type and hierarchy
            if any(pasta in titulo for pasta in PASTAS_TEXTO_NORMAL):
                aplicar_estilo(run, 11, negrito=True)
                p.alignment = WD_PARAGRAPH_ALIGNMENT.JUSTIFY
            elif nivel == 0:
                p.style = 'Heading 1'
            elif nivel == 1:
                p.style = 'Heading 2'
            elif nivel == 2:
                p.style = 'Heading 3'
            else:
                aplicar_estilo(run, 12, negrito=True)
        
        elif isinstance(item, dict):
            if 'imagem' in item:
                # Process image insertion
                imagem_path = item["imagem"]
                if os.path.exists(imagem_path) and os.path.getsize(imagem_path) > 0:
                    try:
                        # Insert image paragraph
                        p = doc.paragraphs[paragrafo_insercao_index].insert_paragraph_before('')
                        
                        # Calculate image dimensions
                        with Image.open(imagem_path) as img:
                            largura_original, altura_original = img.size
                            altura_desejada_cm = 10  # Fixed height as specified
                            
                            # Calculate proportional width
                            ratio = altura_desejada_cm / (altura_original / 28.35)  # Convert pixels to cm
                            largura_proporcional_cm = (largura_original / 28.35) * ratio
                            
                            # Insert image
                            run = p.add_run()
                            run.add_picture(
                                imagem_path,
                                width=Cm(largura_proporcional_cm),
                                height=Cm(altura_desejada_cm)
                            )
                            p.alignment = WD_PARAGRAPH_ALIGNMENT.CENTER
                            contador_imagens += 1
                            
                            # Add line break after image for better spacing
                            p_break = doc.paragraphs[paragrafo_insercao_index].insert_paragraph_before('')
                            p_break.add_run().add_break(WD_BREAK.LINE)
                        
                        # Clean up temporary image file
                        try:
                            os.remove(imagem_path)
                        except:
                            pass  # Ignore cleanup errors
                    
                    except UnidentifiedImageError:
                        print(f"Error: Unrecognized image format: {imagem_path}")
                    except Exception as e:
                        print(f"Error inserting image '{imagem_path}': {e}")
                else:
                    print(f"Error: Invalid image file: {imagem_path}")
            
            elif 'quebra_pagina' in item:
                # Insert page break
                p = doc.paragraphs[paragrafo_insercao_index].insert_paragraph_before('')
                p.add_run().add_break(WD_BREAK.PAGE)
    
    # Save final document
    print(f"Saving document to: {output_path}")
    doc.save(output_path)
    
    print(f"Document created successfully with {contador_imagens} images")
    return contador_imagens
