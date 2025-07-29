# Gerador de Relatórios Fotográficos - MAFFENG

## Overview

**MIGRATED TO C# ASP.NET CORE** - This application was completely refactored from Python Flask to C# ASP.NET Core as requested by the user. The new implementation maintains all original functionality while using modern C# technologies including DocumentFormat.OpenXml for Word processing and SixLabors.ImageSharp for image manipulation.

## Recent Changes

### 2025-07-29 - Complete Migration to C#
- ✅ Migrated from Python Flask to C# ASP.NET Core
- ✅ Implemented preview functionality with drag & drop reordering
- ✅ Created all C# services for ZIP and Word processing
- ✅ Rebuilt all views using Razor templates
- ✅ Maintained identical functionality and user experience
- ✅ Improved performance with native .NET libraries
- ✅ **NEW: Intelligent column layout for images**
  - Small images (≤7.5cm width) grouped in 3-column tables
  - Large images remain in single lines
  - Hybrid layout optimizes document space usage
- → Working on .NET runtime configuration for deployment

## New C# System Architecture

### Frontend Architecture
- **Framework**: ASP.NET Core MVC with Razor views
- **Styling**: TailwindCSS with custom glassmorphism effects (maintained)
- **JavaScript**: Vanilla JS for drag & drop functionality and form validation
- **UI Components**: Responsive design with identical user interface
- **File Upload**: Server-side validation with IFormFile handling

### Backend Architecture
- **Framework**: ASP.NET Core 8.0 (C# web framework)
- **File Processing**: System.IO.Compression for ZIP extraction and organization
- **Document Generation**: DocumentFormat.OpenXml for Word document manipulation
- **Image Processing**: SixLabors.ImageSharp for image handling and optimization
- **Session Management**: ASP.NET Core built-in session handling with JSON serialization
- **Dependency Injection**: Built-in DI container for service management

### C# Services Structure
- **IZipProcessor/ZipProcessor**: Handles ZIP file extraction and folder organization
- **IWordProcessor/WordProcessor**: Manages Word document generation and image insertion
- **IConfigManager/ConfigManager**: Provides configuration data and mappings
- **Controllers/HomeController**: Main MVC controller handling all routes

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
- **Intelligent Image Layout**: 
  - Analyzes image dimensions automatically
  - Groups small images (≤7.5cm width) in 3-column tables
  - Maintains large images in single lines
  - Optimizes space usage and visual appearance
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
- July 07, 2025. Fixed app startup issues and logo display
- July 07, 2025. Implemented placeholder formatting improvements:
  - Arial PT11 for: endereco_dependencia, responsavel_dependencia, responsavel_tecnico
  - Calibri PT11 for: ordem_servico, data_elaboracao, data_atendimento, tipo_atendimento, prefixo_sb, nome_ag, uf
  - Added line breaks after each image for better spacing