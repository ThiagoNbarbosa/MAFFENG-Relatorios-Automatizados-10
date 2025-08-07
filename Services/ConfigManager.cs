namespace MAFFENG.Services
{
    public interface IConfigManager
    {
        Dictionary<string, string> GetAvailableModels();
        List<string> GetBrazilianStates();
        Dictionary<string, string> GetPlaceholders();
    }

    public class ConfigManager : IConfigManager
    {
        public Dictionary<string, string> GetAvailableModels()
        {
            return new Dictionary<string, string>
            {
                { "modelo_3575", "Modelo 3575 - Mato Grosso" },
                { "modelo_6122", "Modelo 6122 - Mato Grosso do Sul" },
                { "modelo_0908", "Modelo 0908 - São Paulo" },
                { "modelo_2056", "Modelo 2056 - Divinópolis" },
                { "modelo_2057", "Modelo 2057 - Varginha" }
            };
        }

        public List<string> GetBrazilianStates()
        {
            return new List<string>
            {
                "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", 
                "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", 
                "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
            };
        }

        public Dictionary<string, string> GetPlaceholders()
        {
            return new Dictionary<string, string>
            {
                { "{{nome_projeto}}", "" },
                { "{{numero_contrato}}", "" },
                { "{{ordem_servico}}", "" },
                { "{{data_elaboracao}}", "" },
                { "{{data_atendimento}}", "" },
                { "{{tipo_atendimento}}", "" },
                { "{{prefixo_sb}}", "" },
                { "{{nome_ag}}", "" },
                { "{{uf}}", "" },
                { "{{endereco_dependencia}}", "" },
                { "{{responsavel_dependencia}}", "" },
                { "{{responsavel_tecnico}}", "" },
                { "{{metodologia_trabalho}}", "" },
                { "{{objetivo_trabalho}}", "" }
            };
        }
    }
}