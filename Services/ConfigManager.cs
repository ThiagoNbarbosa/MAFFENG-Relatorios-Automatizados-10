namespace MAFFENG.Services
{
    public class ConfigManager : IConfigManager
    {
        public Dictionary<string, string> GetAvailableModels()
        {
            return new Dictionary<string, string>
            {
                ["modelo_3575"] = "Modelo 3575 - Mato Grosso",
                ["modelo_6122"] = "Modelo 6122 - Mato Grosso do Sul",
                ["modelo_0908"] = "Modelo 0908 - São Paulo",
                ["modelo_2056"] = "Modelo 2056 - Divinópolis",
                ["modelo_2057"] = "Modelo 2057 - Varginha"
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
                ["{{prefixo_sb}}"] = "prefixo_agencia",
                ["{{nome_ag}}"] = "nome_dependencia",
                ["{{uf}}"] = "uf",
                ["{{numero_contrato}}"] = "numero_contrato",
                ["{{ordem_servico}}"] = "ordem_servico",
                ["{{data_elaboracao}}"] = "data_elaboracao",
                ["{{tipo_atendimento}}"] = "tipo_atendimento",
                ["{{data_atendimento}}"] = "data_elaboracao",
                ["{{endereco_dependencia}}"] = "endereco_completo",
                ["{{responsavel_dependencia}}"] = "responsavel_dependencia",
                ["{{responsavel_tecnico}}"] = "responsavel_tecnico",
                ["{{elaborado_por}}"] = "Ygor Augusto Fernandes",
                ["{{empresa}}"] = "MAFFENG - Engenharia e Manutenção Profissional"
            };
        }
    }
}