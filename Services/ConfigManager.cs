namespace MAFFENG.Services
{
    public interface IConfigManager
    {
        Dictionary<string, string> GetAvailableModels();
        Dictionary<string, string> GetBrazilianStates();
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

        public Dictionary<string, string> GetBrazilianStates()
        {
            return new Dictionary<string, string>
            {
                { "AC", "Acre" },
                { "AL", "Alagoas" },
                { "AP", "Amapá" },
                { "AM", "Amazonas" },
                { "BA", "Bahia" },
                { "CE", "Ceará" },
                { "DF", "Distrito Federal" },
                { "ES", "Espírito Santo" },
                { "GO", "Goiás" },
                { "MA", "Maranhão" },
                { "MT", "Mato Grosso" },
                { "MS", "Mato Grosso do Sul" },
                { "MG", "Minas Gerais" },
                { "PA", "Pará" },
                { "PB", "Paraíba" },
                { "PR", "Paraná" },
                { "PE", "Pernambuco" },
                { "PI", "Piauí" },
                { "RJ", "Rio de Janeiro" },
                { "RN", "Rio Grande do Norte" },
                { "RS", "Rio Grande do Sul" },
                { "RO", "Rondônia" },
                { "RR", "Roraima" },
                { "SC", "Santa Catarina" },
                { "SP", "São Paulo" },
                { "SE", "Sergipe" },
                { "TO", "Tocantins" }
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