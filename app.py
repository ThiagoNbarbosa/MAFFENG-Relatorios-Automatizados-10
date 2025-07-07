import os
import logging
import zipfile
import shutil
from datetime import datetime
from flask import Flask, render_template, request, redirect, url_for, flash, send_from_directory, jsonify
from werkzeug.utils import secure_filename
from werkzeug.middleware.proxy_fix import ProxyFix
from word_utils import processar_zip, inserir_conteudo_word, substituir_placeholders
from docx_formatter import aplicar_melhorias_formatacao

# Configure logging
logging.basicConfig(level=logging.DEBUG)

# Create Flask app
app = Flask(__name__)
app.secret_key = os.environ.get("SESSION_SECRET", "dev-secret-key-change-in-production")
app.wsgi_app = ProxyFix(app.wsgi_app, x_proto=1, x_host=1)

# Configuration
app.config['MAX_CONTENT_LENGTH'] = 100 * 1024 * 1024  # 100MB max file size
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
    """Validate required form fields"""
    required_fields = [
        'nome_projeto', 'numero_contrato', 'ordem_servico', 'data_elaboracao',
        'prefixo_agencia', 'nome_dependencia', 'endereco_completo', 'uf',
        'tipo_atendimento', 'responsavel_dependencia', 'responsavel_tecnico',
        'modelo_selecionado'
    ]
    
    errors = []
    for field in required_fields:
        if not form_data.get(field):
            errors.append(f'Campo obrigatório: {field.replace("_", " ").title()}')
    
    return errors

@app.route('/')
def index():
    """Main page with form"""
    return render_template('index.html', 
                         modelos=AVAILABLE_MODELS, 
                         estados=ESTADOS_BRASIL)

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
        filename = secure_filename(file.filename)
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
        
        # Generate output filename
        nome_projeto = form_data['nome_projeto']
        safe_project_name = secure_filename(nome_projeto)
        output_filename = f"RELATÓRIO FOTOGRÁFICO - {safe_project_name} - LEVANTAMENTO PREVENTIVO.docx"
        output_path = os.path.join(app.config['OUTPUT_FOLDER'], output_filename)
        
        # Generate Word document
        app.logger.info(f"Generating Word document: {output_path}")
        num_imagens = inserir_conteudo_word(modelo_path, conteudo_estruturado, PLACEHOLDERS, form_data, output_path)
        
        # Clean up temporary files
        os.remove(zip_path)
        
        # Success message
        flash(f'Relatório gerado com sucesso! {num_imagens} imagens inseridas.', 'success')
        
        return render_template('success.html', 
                             filename=output_filename,
                             num_imagens=num_imagens,
                             projeto=nome_projeto)
        
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

@app.route('/formatar-documento', methods=['GET', 'POST'])
def formatar_documento():
    """Página para formatação de documentos existentes"""
    if request.method == 'GET':
        return render_template('formatar.html')
    
    try:
        # Verificar se arquivo foi enviado
        if 'arquivo_docx' not in request.files:
            flash('Nenhum arquivo .docx selecionado', 'error')
            return redirect(url_for('formatar_documento'))
        
        file = request.files['arquivo_docx']
        if file.filename == '':
            flash('Nenhum arquivo selecionado', 'error')
            return redirect(url_for('formatar_documento'))
        
        if not file.filename.lower().endswith('.docx'):
            flash('Apenas arquivos .docx são permitidos', 'error')
            return redirect(url_for('formatar_documento'))
        
        # Salvar arquivo temporário
        filename = secure_filename(file.filename)
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        temp_filename = f"temp_{timestamp}_{filename}"
        temp_path = os.path.join(app.config['UPLOAD_FOLDER'], temp_filename)
        file.save(temp_path)
        
        # Gerar nome do arquivo formatado
        base_name = os.path.splitext(filename)[0]
        output_filename = f"{base_name}_FORMATADO.docx"
        output_path = os.path.join(app.config['OUTPUT_FOLDER'], output_filename)
        
        # Aplicar formatação
        sucesso = aplicar_melhorias_formatacao(temp_path, output_path)
        
        # Limpar arquivo temporário
        os.remove(temp_path)
        
        if sucesso:
            flash('Documento formatado com sucesso!', 'success')
            return render_template('success_formatacao.html', filename=output_filename)
        else:
            flash('Erro ao formatar documento', 'error')
            return redirect(url_for('formatar_documento'))
    
    except Exception as e:
        app.logger.error(f"Erro na formatação: {str(e)}")
        flash(f'Erro ao processar arquivo: {str(e)}', 'error')
        return redirect(url_for('formatar_documento'))

@app.errorhandler(413)
def too_large(e):
    """Handle file too large error"""
    flash('Arquivo muito grande. Tamanho máximo: 100MB', 'error')
    return redirect(url_for('index'))

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
