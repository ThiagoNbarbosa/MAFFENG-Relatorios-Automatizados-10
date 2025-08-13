# MAFFENG - Gerador de Relatórios Fotográficos (C# ASP.NET Core)

## 🚀 Migração Completa para C#

Este projeto foi **completamente migrado de Python Flask para C# ASP.NET Core** mantendo todas as funcionalidades originais e adicionando a nova funcionalidade de preview com reordenação.

## ✨ Funcionalidades

### Principais
- Upload de arquivos ZIP com fotos organizadas em pastas
- Processamento automático de estrutura de pastas
- **Preview interativo** com possibilidade de reordenar, editar títulos e remover itens
- Geração automática de relatórios em formato Word (.docx)
- Substituição automática de placeholders com dados do formulário
- Interface responsiva com design glassmorphism

### Funcionalidades da Página de Preview
- 📋 Visualização completa da estrutura antes da geração
- ✏️ Edição de títulos das seções
- 🔄 Reordenação via drag & drop
- 🗑️ Remoção de itens desnecessários
- ✅ Confirmação antes da geração final

## 🏗️ Arquitetura C#

### Tecnologias Utilizadas
- **ASP.NET Core 8.0** - Framework web principal
- **DocumentFormat.OpenXml** - Manipulação de documentos Word
- **SixLabors.ImageSharp** - Processamento de imagens
- **System.IO.Compression** - Processamento de arquivos ZIP
- **TailwindCSS** - Framework CSS para design responsivo

### Estrutura do Projeto
```
/
├── Program.cs                  # Configuração e inicialização da aplicação
├── MAFFENG.csproj             # Configuração do projeto e dependências
├── Controllers/
│   └── HomeController.cs      # Controller principal com todas as rotas
├── Models/
│   └── UploadFormModel.cs     # Modelos de dados e DTOs
├── Services/
│   ├── IZipProcessor.cs       # Interface para processamento ZIP
│   ├── ZipProcessor.cs        # Implementação do processamento ZIP
│   ├── IWordProcessor.cs      # Interface para geração de documentos
│   ├── WordProcessor.cs       # Implementação da geração Word
│   ├── IConfigManager.cs      # Interface para configurações
│   └── ConfigManager.cs       # Gerenciamento de configurações
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml     # Layout principal da aplicação
│   └── Home/
│       ├── Index.cshtml       # Página principal com formulário
│       ├── Preview.cshtml     # Página de preview com funcionalidades
│       └── Success.cshtml     # Página de sucesso com download
├── wwwroot/
│   ├── css/                   # Arquivos CSS customizados
│   ├── js/                    # Scripts JavaScript
│   └── images/                # Imagens e ícones
├── models/                    # Templates Word (.docx)
├── uploads/                   # Diretório temporário para uploads
└── output/                    # Diretório para relatórios gerados
```

## 🔧 Configuração e Execução

### Pré-requisitos
- .NET 8.0 SDK
- Sistema operacional compatível (Windows/Linux/macOS)

### Comandos de Execução
```bash
# Restaurar dependências
dotnet restore

# Executar aplicação
dotnet run

# A aplicação estará disponível em: http://localhost:5000
```

### Variáveis de Ambiente
```bash
# Opcional: Configurar porta específica
export ASPNETCORE_URLS="http://0.0.0.0:5000"
```

## 📋 Fluxo de Uso

1. **Upload**: Usuário faz upload do ZIP e preenche formulário
2. **Processamento**: Sistema extrai e organiza estrutura de pastas
3. **Preview**: Página interativa mostra estrutura e permite personalização
4. **Geração**: Sistema gera documento Word com layout personalizado
5. **Download**: Usuário baixa o relatório finalizado

## 🎯 Melhorias na Migração C#

### Performance
- Processamento mais rápido com bibliotecas nativas .NET
- Melhor gerenciamento de memória para arquivos grandes
- Processamento assíncrono de imagens

### Manutenibilidade
- Arquitetura baseada em serviços com injeção de dependência
- Separação clara de responsabilidades
- Interfaces bem definidas para testabilidade
- Validação robusta com Data Annotations

### Funcionalidades Novas
- Preview interativo com reordenação
- Drag & drop para reorganizar estrutura
- Edição inline de títulos das seções
- Validação melhorada no frontend e backend

## 📝 Modelos Suportados

- **Modelo 3575** - Mato Grosso
- **Modelo 6122** - Mato Grosso do Sul  
- **Modelo 0908** - São Paulo
- **Modelo 2056** - Divinópolis
- **Modelo 2057** - Varginha

## 🔒 Segurança

- Validação rigorosa de tipos de arquivo
- Sanitização de nomes de arquivo
- Limpeza automática de arquivos temporários
- Validação de integridade de arquivos ZIP
- Proteção contra ataques de path traversal

## 📞 Suporte

Para questões técnicas relacionadas à migração ou funcionamento da aplicação C#, consulte a documentação do projeto ou entre em contato com a equipe de desenvolvimento.