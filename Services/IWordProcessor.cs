using MAFFENG.Models;

namespace MAFFENG.Services
{
    public interface IWordProcessor
    {
        Task<int> GenerateReportAsync(string templatePath, List<object> content, Dictionary<string, string> placeholders, UploadFormModel formData, string outputPath);
    }
}