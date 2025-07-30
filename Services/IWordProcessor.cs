using MAFFENG.Models;

namespace MAFFENG.Services
{
    public interface IWordProcessor
    {
        Task<int> GenerateReportAsync(string modelPath, List<object> content, Dictionary<string, string> placeholders, UploadFormModel formData, string outputPath);
    }
}