import os
import logging
import zipfile
import shutil
from datetime import datetime
from flask import Flask, render_template, request, redirect, url_for, flash, send_from_directory, jsonify, session
from werkzeug.utils import secure_filename
from werkzeug.middleware.proxy_fix import ProxyFix
from word_utils import processar_zip, inserir_conteudo_word, substituir_placeholders
from config_manager import config_manager

# Configure logging
logging.basicConfig(level=logging.DEBUG)

# Create Flask app
app = Flask(__name__)
app.secret_key = os.environ.get("SESSION_SECRET", "dev-secret-key-change-in-production")
app.wsgi_app = ProxyFix(app.wsgi_app, x_proto=1, x_host=1)

# Configuration
app.config['MAX_CONTENT_LENGTH'] = 500 * 1024 * 1024  # 500MB max file size
app.config['UPLOAD_FOLDER'] = 'uploads'
app.config['OUTPUT_FOLDER'] = 'output'
app.config['MODELS_FOLDER'] = 'models'

# Ensure required directories exist
os.makedirs(app.config['UPLOAD_FOLDER'], exist_ok=True)
os.makedirs(app.config['OUTPUT_FOLDER'], exist_ok=True)
os.makedirs(app.config['MODELS_FOLDER'], exist_ok=True)

# Allowed file extensions
ALLOWED_EXTENSIONS = {'zip'}

# Available models mapping
AVAILABLE_MODELS = {
    'modelo_3575': 'Modelo 3575 - Mato Grosso',
    'modelo_6122': 'Modelo 6122 - Mato Grosso do Sul',
    'modelo_0908': 'Modelo 0908 - São Paulo',
    'modelo_2056': 'Modelo 2056 - Divinópolis',
    'modelo_2057': 'Modelo 2057 - Varginha'
}

# Brazilian states for UF dropdown
ESTADOS_BRASIL = [
    'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 
    'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 
    'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
]

# Placeholder mapping as specified
PLACEHOLDERS = {
    '{{prefixo_sb}}': 'prefixo_agencia',
    '{{nome_ag}}': 'nome_dependencia',  # Note: template uses {{nome_ag}} but form field is nome_dependencia
    '{{uf}}': 'uf',
    '{{numero_contrato}}': 'numero_contrato',
    '{{ordem_servico}}': 'ordem_servico',
    '{{data_elaboracao}}': 'data_elaboracao',
    '{{tipo_atendimento}}': 'tipo_atendimento',
    '{{data_atendimento}}': 'data_elaboracao',  # Same as elaboracao
    '{{endereco_dependencia}}': 'endereco_completo',
    '{{responsavel_dependencia}}': 'responsavel_dependencia',
    '{{responsavel_tecnico}}': 'responsavel_tecnico',
    '{{elaborado_por}}': 'Ygor Augusto Fernandes',  # Fixed value
    '{{empresa}}': 'MAFFENG - Engenharia e Manutenção Profissional'  # Fixed value
}

def allowed_file(filename):
    """Check if file extension is allowed"""
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS

def validate_form_data(form_data):
    """Validate required form fields based on configuration"""
    errors = []
    
    # Validar campos obrigatórios básicos
    basic_required = ['nome_projeto', 'modelo_selecionado']
    for field in basic_required:
        if not form_data.get(field):
            errors.append(f'Campo obrigatório: {field.replace("_", " ").title()}')
    
    # Validar campos variáveis obrigatórios
    variable_fields = config_manager.get_variable_fields()
    for field_key, config in variable_fields.items():
        if config.get('required', False) and not form_data.get(field_key):
            label = config.get('label', field_key.replace('_', ' ').title())
            errors.append(f'Campo obrigatório: {label}')
    
    return errors

@app.route('/')
def index():
    """Main page with form"""
    # Obter configuração organizada por seções
    placeholders = config_manager.get_all_placeholders()
    
    section_icons = {
        'projeto': 'fa-project-diagram',
        'dependencia': 'fa-building',
        'responsaveis': 'fa-users',
        'empresa': 'fa-industry'
    }
    
    return render_template('index.html', 
                         modelos=AVAILABLE_MODELS, 
                         estados=ESTADOS_BRASIL,
                         placeholders=placeholders,
                         section_icons=section_icons)

@app.route('/configuracoes')
def configuracoes():
    """Página de configuração de placeholders"""
    placeholders = config_manager.get_all_placeholders()
    
    section_icons = {
        'projeto': 'fa-project-diagram',
        'dependencia': 'fa-building',
        'responsaveis': 'fa-users',
        'empresa': 'fa-industry'
    }
    
    # Formatação da data de última atualização
    last_updated = 'Nunca'
    if 'updated_at' in config_manager.config:
        try:
            from datetime import datetime
            dt = datetime.fromisoformat(config_manager.config['updated_at'])
            last_updated = dt.strftime('%d/%m/%Y às %H:%M')
        except:
            pass
    
    return render_template('config.html',
                         placeholders=placeholders,
                         section_icons=section_icons,
                         last_updated=last_updated)

@app.route('/configuracoes/salvar', methods=['POST'])
def salvar_configuracoes():
    """Salva as configurações dos placeholders"""
    try:
        form_data = request.form.to_dict()
        
        # Processar dados do formulário
        for key, config in config_manager.config['placeholders'].items():
            field_type = form_data.get(f'type_{key}', config.get('type', 'variable'))
            label = form_data.get(f'label_{key}', config.get('label', ''))
            value = form_data.get(f'value_{key}', config.get('value', ''))
            
            # Validar campos fixos obrigatórios
            if field_type == 'fixed' and not value.strip():
                flash(f'Valor fixo obrigatório para: {label}', 'error')
                return redirect(url_for('configuracoes'))
            
            # Atualizar configuração
            config_manager.update_placeholder(key, field_type, value, label)
        
        flash('Configurações salvas com sucesso!', 'success')
        
    except Exception as e:
        flash(f'Erro ao salvar configurações: {str(e)}', 'error')
        app.logger.error(f"Erro ao salvar configurações: {e}")
    
    return redirect(url_for('configuracoes'))

@app.route('/configuracoes/reset')
def reset_configuracoes():
    """Restaura configurações padrão"""
    try:
        # Recriar configuração padrão
        config_manager.config = config_manager.default_config.copy()
        config_manager.save_config()
        flash('Configurações restauradas para o padrão!', 'success')
    except Exception as e:
        flash(f'Erro ao restaurar configurações: {str(e)}', 'error')
    
    return redirect(url_for('configuracoes'))

@app.route('/preview')
def preview():
    """Preview page showing folder structure and images before generating report"""
    if 'temp_content_file' not in session:
        flash('Dados não encontrados. Por favor, faça o upload novamente.', 'error')
        return redirect(url_for('index'))
    
    # Load content from temporary file
    import pickle
    temp_content_file = session['temp_content_file']
    try:
        with open(temp_content_file, 'rb') as f:
            conteudo_estruturado = pickle.load(f)
    except FileNotFoundError:
        flash('Dados temporários não encontrados. Por favor, faça upload novamente.', 'error')
        return redirect(url_for('index'))
    
    form_data = session['form_data']
    
    # Organize content for preview
    preview_items = []
    current_folder = None
    
    for item in conteudo_estruturado:
        if isinstance(item, str):
            # This is a folder title
            nivel = item.count("»")
            titulo_limpo = item.replace("»", "").strip()
            if titulo_limpo.startswith("- -"):
                titulo_limpo = titulo_limpo[2:].strip()
            
            current_folder = {
                'type': 'folder',
                'title': titulo_limpo,
                'original_title': item,
                'level': nivel,
                'images': []
            }
            preview_items.append(current_folder)
            
        elif isinstance(item, dict) and 'imagem' in item:
            # This is an image
            if current_folder:
                image_name = os.path.basename(item['imagem'])
                current_folder['images'].append({
                    'type': 'image',
                    'name': image_name,
                    'path': item['imagem']
                })
        elif isinstance(item, dict) and 'quebra_pagina' in item:
            # Skip page breaks in preview
            continue
    
    return render_template('preview.html', 
                         preview_items=preview_items,
                         form_data=form_data)

@app.route('/generate-report', methods=['POST'])
def generate_report():
    """Generate the final report with customized order"""
    if 'form_data' not in session:
        flash('Dados não encontrados. Por favor, faça o upload novamente.', 'error')
        return redirect(url_for('index'))
    
    try:
        # Get data from session
        form_data = session['form_data']
        zip_path = session['zip_path']
        modelo_path = session['modelo_path']
        
        # Get customized order from form
        customized_content = []
        form_data_from_request = request.form.to_dict()
        
        # Rebuild content structure based on form data
        i = 0
        while f'item_type_{i}' in form_data_from_request:
            item_type = form_data_from_request[f'item_type_{i}']
            
            if item_type == 'folder':
                title = form_data_from_request[f'item_title_{i}']
                original_title = form_data_from_request[f'item_original_{i}']
                level = int(form_data_from_request[f'item_level_{i}'])
                
                # Reconstruct folder title with proper level markers
                if level == 0:
                    folder_title = title
                elif level == 1:
                    folder_title = f"»{title}"
                elif level == 2:
                    folder_title = f"»»{title}"
                else:
                    folder_title = f"»»»{title}"
                
                customized_content.append(folder_title)
                
            elif item_type == 'image':
                image_path = form_data_from_request[f'item_path_{i}']
                customized_content.append({"imagem": image_path})
            
            i += 1
        
        # Add page breaks between sections (similar to original logic)
        final_content = []
        current_folder_images = []
        
        for item in customized_content:
            if isinstance(item, str):
                # If we have accumulated images, add them and a page break
                if current_folder_images:
                    final_content.extend(current_folder_images)
                    final_content.append({"quebra_pagina": True})
                    current_folder_images = []
                final_content.append(item)
            else:
                current_folder_images.append(item)
        
        # Add remaining images
        if current_folder_images:
            final_content.extend(current_folder_images)
            final_content.append({"quebra_pagina": True})
        
        # Merge form data with fixed values from configuration
        complete_form_data = config_manager.get_form_data_with_defaults(form_data)

        # Generate output filename
        nome_projeto = form_data['nome_projeto']
        safe_project_name = secure_filename(nome_projeto)
        output_filename = f"RELATÓRIO FOTOGRÁFICO - {safe_project_name} - LEVANTAMENTO PREVENTIVO.docx"
        output_path = os.path.join(app.config['OUTPUT_FOLDER'], output_filename)

        # Generate Word document
        app.logger.info(f"Generating Word document: {output_path}")
        num_imagens = inserir_conteudo_word(modelo_path, final_content, PLACEHOLDERS, complete_form_data, output_path)

        # Clean up temporary files
        if os.path.exists(zip_path):
            os.remove(zip_path)

        # Clean up temporary content file
        temp_content_file = session.get('temp_content_file')
        if temp_content_file and os.path.exists(temp_content_file):
            try:
                os.remove(temp_content_file)
            except Exception as e:
                app.logger.warning(f"Could not remove temporary file {temp_content_file}: {e}")
        
        # Clear session data
        session.pop('form_data', None)
        session.pop('zip_path', None)
        session.pop('conteudo_count', None)
        session.pop('modelo_path', None)
        session.pop('temp_content_file', None)

        # Success message
        flash(f'Relatório gerado com sucesso! {num_imagens} imagens inseridas.', 'success')

        return render_template('success.html', 
                             filename=output_filename,
                             num_imagens=num_imagens,
                             projeto=nome_projeto)

    except Exception as e:
        app.logger.error(f"Error generating report: {str(e)}")
        flash(f'Erro ao gerar relatório: {str(e)}', 'error')
        return redirect(url_for('preview'))

@app.route('/upload', methods=['POST'])
def processar_upload():
    """Process form submission and ZIP file upload"""
    try:
        # Validate form data
        form_data = request.form.to_dict()
        errors = validate_form_data(form_data)

        if errors:
            for error in errors:
                flash(error, 'error')
            return redirect(url_for('index'))

        # Check if file was uploaded
        if 'arquivo_zip' not in request.files:
            flash('Nenhum arquivo ZIP selecionado', 'error')
            return redirect(url_for('index'))

        file = request.files['arquivo_zip']
        if file.filename == '':
            flash('Nenhum arquivo selecionado', 'error')
            return redirect(url_for('index'))

        if not allowed_file(file.filename):
            flash('Apenas arquivos ZIP são permitidos', 'error')
            return redirect(url_for('index'))

        # Save uploaded file
        filename = secure_filename(file.filename or 'arquivo.zip')
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        safe_filename = f"{timestamp}_{filename}"
        zip_path = os.path.join(app.config['UPLOAD_FOLDER'], safe_filename)
        file.save(zip_path)

        # Validate ZIP file
        try:
            with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                zip_ref.testzip()
        except zipfile.BadZipFile:
            flash('Arquivo ZIP corrompido ou inválido', 'error')
            os.remove(zip_path)
            return redirect(url_for('index'))

        # Get selected model path
        modelo_selecionado = form_data['modelo_selecionado']
        modelo_path = os.path.join(app.config['MODELS_FOLDER'], f"{modelo_selecionado}.docx")

        if not os.path.exists(modelo_path):
            flash(f'Modelo não encontrado: {modelo_selecionado}', 'error')
            os.remove(zip_path)
            return redirect(url_for('index'))

        # Process ZIP file
        app.logger.info(f"Processing ZIP file: {zip_path}")
        conteudo_estruturado = processar_zip(zip_path, form_data)

        # Store data in session for preview page (optimized for size)
        session['form_data'] = form_data
        session['zip_path'] = zip_path
        # Store only structure, not the full image paths to reduce session size
        session['conteudo_count'] = len(conteudo_estruturado)
        session['modelo_path'] = modelo_path
        
        # Store content in a temporary file instead of session
        import tempfile, pickle
        temp_content_file = tempfile.NamedTemporaryFile(delete=False, suffix='.pkl')
        with open(temp_content_file.name, 'wb') as f:
            pickle.dump(conteudo_estruturado, f)
        session['temp_content_file'] = temp_content_file.name

        # Redirect to preview page
        return redirect(url_for('preview'))

    except Exception as e:
        app.logger.error(f"Error processing upload: {str(e)}")
        flash(f'Erro ao processar arquivo: {str(e)}', 'error')
        return redirect(url_for('index'))

@app.route('/download/<filename>')
def download_file(filename):
    """Download generated report"""
    try:
        return send_from_directory(app.config['OUTPUT_FOLDER'], filename, as_attachment=True)
    except FileNotFoundError:
        flash('Arquivo não encontrado', 'error')
        return redirect(url_for('index'))

@app.errorhandler(413)
def too_large(e):
    """Handle file too large error"""
    flash('Arquivo muito grande. Tamanho máximo: 500MB', 'error')
    return redirect(url_for('index'))

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)