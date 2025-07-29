using MAFFENG.Models;

namespace MAFFENG.Services
{
    public interface IZipProcessor
    {
        Task<List<object>> ProcessZipAsync(string zipPath, UploadFormModel formData);
    }
}