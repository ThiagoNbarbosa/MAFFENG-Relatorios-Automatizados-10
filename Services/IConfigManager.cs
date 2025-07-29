namespace MAFFENG.Services
{
    public interface IConfigManager
    {
        Dictionary<string, string> GetAvailableModels();
        List<string> GetBrazilianStates();
        Dictionary<string, string> GetPlaceholders();
    }
}