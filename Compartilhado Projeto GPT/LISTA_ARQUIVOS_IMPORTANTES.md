
# LISTA DOS 20 ARQUIVOS MAIS IMPORTANTES - APLICATIVO MAFFENG

## 📋 ARQUIVOS PRINCIPAIS DO SISTEMA

### 1. **app.py** - Núcleo da Aplicação
- Arquivo principal do Flask
- Rotas, configurações e lógica de negócio
- Gerenciamento de uploads e validações

### 2. **word_utils.py** - Processamento de Documentos
- Lógica de processamento de ZIP
- Inserção de imagens em documentos Word
- Substituição de placeholders

### 3. **config_manager.py** - Gerenciador de Configurações
- Sistema de configuração dinâmica
- Gerenciamento de placeholders
- Persistência de configurações

### 4. **main.py** - Ponto de Entrada
- Arquivo de inicialização da aplicação
- Configuração do servidor

### 5. **templates/base.html** - Template Base
- Layout principal da aplicação
- Navegação e estrutura HTML
- Integração com TailwindCSS

### 6. **templates/index.html** - Página Principal
- Formulário de upload
- Interface de entrada de dados
- Validações client-side

### 7. **templates/config.html** - Página de Configurações
- Interface de gerenciamento de placeholders
- Configuração de campos fixos e variáveis

### 8. **templates/success.html** - Página de Sucesso
- Feedback de processamento
- Link de download do relatório

### 9. **static/css/style.css** - Estilos Customizados
- Efeitos glassmorphism
- Responsividade
- Animações e transições

### 10. **static/js/main.js** - JavaScript Principal
- Validação de formulários
- Upload de arquivos
- Feedback visual

### 11. **config/placeholders_config.json** - Configurações
- Estrutura de dados dos placeholders
- Configurações de seções
- Valores padrão

### 12. **pyproject.toml** - Dependências
- Configuração do projeto Python
- Lista de dependências necessárias

### 13. **models/modelo_3575.docx** - Template Mato Grosso
- Template Word para região MT

### 14. **models/modelo_6122.docx** - Template Mato Grosso do Sul
- Template Word para região MS

### 15. **models/modelo_0908.docx** - Template São Paulo
- Template Word para região SP

### 16. **models/modelo_2056.docx** - Template Divinópolis
- Template Word para Divinópolis

### 17. **models/modelo_2057.docx** - Template Varginha
- Template Word para Varginha

### 18. **static/images/maffeng-logo.png** - Logo da Empresa
- Logotipo oficial da MAFFENG
- Usado no cabeçalho da aplicação

### 19. **static/images/favicon.png** - Ícone do Site
- Favicon da aplicação
- Identidade visual

### 20. **replit.md** - Documentação Técnica
- Documentação da arquitetura
- Informações de deployment
- Guia técnico do sistema

## 🎯 IMPORTÂNCIA DE CADA ARQUIVO

### **CRÍTICOS (Não funciona sem eles)**
- app.py, word_utils.py, config_manager.py, main.py

### **ESSENCIAIS (Interface e configuração)**
- templates/*.html, config/placeholders_config.json

### **IMPORTANTES (Experiência do usuário)**
- static/css/style.css, static/js/main.js, static/images/*

### **FUNCIONAIS (Templates de documentos)**
- models/*.docx

### **SUPORTE (Documentação e configuração)**
- pyproject.toml, replit.md

## 📊 ESTATÍSTICAS DO PROJETO

- **Total de linhas de código**: ~2.500 linhas
- **Linguagens**: Python (60%), HTML/CSS (25%), JavaScript (15%)
- **Frameworks**: Flask, TailwindCSS
- **Funcionalidades**: 12 módulos principais
- **Templates Word**: 5 modelos regionais
- **Capacidade**: Processamento de até 500MB
