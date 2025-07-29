using System.ComponentModel.DataAnnotations;

namespace MAFFENG.Models
{
    public class UploadFormModel
    {
        [Required(ErrorMessage = "Nome do projeto é obrigatório")]
        public string NomeProjeto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Modelo é obrigatório")]
        public string ModeloSelecionado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arquivo ZIP é obrigatório")]
        public IFormFile? ArquivoZip { get; set; }

        // Campos dinâmicos baseados na configuração
        public string? NumeroContrato { get; set; }
        public string? OrdemServico { get; set; }
        public DateTime? DataElaboracao { get; set; } = DateTime.Now;
        public string? PrefixoAgencia { get; set; }
        public string? NomeDependencia { get; set; }
        public string? UF { get; set; }
        public string? EnderecoCompleto { get; set; }
        public string? ResponsavelDependencia { get; set; }
        public string? ResponsavelTecnico { get; set; }
        public string? TipoAtendimento { get; set; } = "LEVANTAMENTO PREVENTIVO";
    }

    public class PreviewItem
    {
        public string Type { get; set; } = string.Empty; // "folder" ou "image"
        public string Title { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public int Level { get; set; }
        public string? Path { get; set; }
        public List<PreviewImage> Images { get; set; } = new();
    }

    public class PreviewImage
    {
        public string Type { get; set; } = "image";
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class PreviewModel
    {
        public List<PreviewItem> PreviewItems { get; set; } = new();
        public UploadFormModel FormData { get; set; } = new();
    }
}