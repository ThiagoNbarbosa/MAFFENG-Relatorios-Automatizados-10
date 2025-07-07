# Gerador de Relatórios Fotográficos - MAFFENG

## Overview

This is a Flask-based web application designed to automate the generation of professional photographic reports for MAFFENG company. The system processes ZIP files containing organized photo folders and generates Word document reports with proper formatting, image insertion, and placeholder replacement.

## System Architecture

### Frontend Architecture
- **Framework**: Flask with Jinja2 templates
- **Styling**: TailwindCSS with custom glassmorphism effects
- **JavaScript**: Vanilla JS for form validation and file handling
- **UI Components**: Responsive design with four main sections for data input
- **File Upload**: Client-side validation for ZIP files with 100MB size limit

### Backend Architecture
- **Framework**: Flask (Python web framework)
- **File Processing**: Custom ZIP extraction and folder organization
- **Document Generation**: Python-docx library for Word document manipulation
- **Image Processing**: PIL (Pillow) for image handling and optimization
- **Session Management**: Flask built-in session handling with secure keys

### Data Flow
1. User uploads ZIP file through web interface
2. Form data is validated on both client and server side
3. ZIP file is extracted to temporary directory
4. Folder structure is analyzed and organized according to predefined order
5. Images are processed and inserted into Word template
6. Placeholders are replaced with form data
7. Final document is generated and made available for download

## Key Components

### File Structure
```
/app.py - Main Flask application with routing and configuration
/word_utils.py - Document processing utilities and image handling
/templates/ - HTML templates (base, index, success)
/static/ - CSS, JS, and image assets
/uploads/ - Temporary file storage
/output/ - Generated reports storage
/models/ - Word document templates
```

### Core Processing Logic
- **Folder Organization**: Processes folders in specific order (Área externa, Área interna, Segundo piso, Detalhes, Vista ampla)
- **Image Handling**: Supports PNG, JPG, JPEG formats with automatic resizing
- **Template Processing**: Uses placeholder replacement for dynamic content insertion
- **Document Structure**: Maintains hierarchical heading structure based on folder depth

### Form Data Structure
The application collects data in four main sections:
1. **Project Information**: Name, contract number, service order, elaboration date
2. **Dependency Information**: Agency prefix, dependency name, address details
3. **Technical Information**: Responsible engineer, methodology, objectives
4. **File Upload**: ZIP file containing organized photo folders

## Data Storage

### File Management
- **Temporary Storage**: Upload folder for processing ZIP files
- **Output Storage**: Generated reports stored in output folder
- **Template Storage**: Word document templates stored in models folder
- **Session Storage**: Form data temporarily stored in Flask sessions

### Folder Organization Rules
- Predefined folder processing order ensures consistent report structure
- Special handling for "Detalhes" and "Vista ampla" folders (normal text instead of headings)
- Hierarchical structure maintained with proper heading levels

## External Dependencies

### Python Libraries
- **Flask**: Web framework and routing
- **python-docx**: Word document manipulation
- **Pillow (PIL)**: Image processing and optimization
- **Werkzeug**: File handling and security utilities

### Frontend Dependencies
- **TailwindCSS**: Styling framework (CDN)
- **Font Awesome**: Icon library (CDN)
- **Custom CSS**: Glassmorphism effects and responsive design

### System Requirements
- Python 3.7+
- Modern web browser with JavaScript support
- Adequate disk space for temporary file processing

## Deployment Strategy

### Configuration
- Environment-based secret key management
- Configurable upload and output directories
- File size limits and allowed extensions
- Proxy fix for deployment behind reverse proxy

### Security Measures
- Secure filename handling to prevent directory traversal
- File type validation (ZIP only)
- File size limits (100MB maximum)
- Session security with configurable secret keys

### Production Considerations
- WSGI application with ProxyFix middleware
- Logging configuration for debugging
- Error handling and user feedback
- Temporary file cleanup

## User Preferences

Preferred communication style: Simple, everyday language.

## Changelog

Changelog:
- July 07, 2025. Initial setup
- July 07, 2025. Implemented advanced document formatting improvements:
  - All placeholders now use Calibri 11pt font for consistency
  - Added automatic line breaks after each image for better spacing
  - Implemented intelligent image organization based on width:
    - Images ≤ 5.92cm: organized in 3 columns using invisible tables
    - Images ≤ 7.50cm: organized in 2 columns using invisible tables
    - Images > 7.50cm: displayed in single column (full width)
  - Added proper paragraph spacing (12pt) after all elements
  - Preserved all original content and hierarchical structure
  - Enhanced visual presentation while maintaining document integrity