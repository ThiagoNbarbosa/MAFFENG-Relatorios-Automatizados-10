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

        // Propriedades do formulário completo (mantidas por compatibilidade)
        public string NumeroContrato { get; set; } = string.Empty;
        public string OrdemServico { get; set; } = string.Empty;
        public DateTime DataElaboracao { get; set; } = DateTime.Now;
        public DateTime DataAtendimento { get; set; } = DateTime.Now;
        public string TipoAtendimento { get; set; } = string.Empty;
        public string PrefixoSB { get; set; } = string.Empty;
        public string NomeAG { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;
        public string EnderecoCompleto { get; set; } = string.Empty;
        public string ResponsavelDependencia { get; set; } = string.Empty;
        public string ResponsavelTecnico { get; set; } = string.Empty;
        public string MetodologiaTrabalho { get; set; } = string.Empty;
        public string ObjetivoTrabalho { get; set; } = string.Empty;
    }
}