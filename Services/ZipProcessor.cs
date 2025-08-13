using System.IO.Compression;
using MAFFENG.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MAFFENG.Services
{
    public interface IZipProcessor
    {
        Task<PreviewData> ProcessZipAsync(string zipPath, UploadFormModel formData);
        Task<List<object>> ExtractAndOrganizeAsync(string zipPath);
        Task<string> GenerateThumbnailAsync(string imagePath, int width = 150, int height = 150);
    }

    public class ZipProcessor : IZipProcessor
    {
        private readonly ILogger<ZipProcessor> _logger;
        private readonly IWebHostEnvironment _environment;

        // Folder processing order as specified
        private static readonly string[] ORDEM_PASTAS = {
            "- Área externa",
            "- Área interna", 
            "- Segundo piso",
            "- Detalhes",
            "- Vista ampla"
        };

        // Folders that should use normal text with bold instead of headings
        private static readonly string[] PASTAS_TEXTO_NORMAL = { "- Detalhes", "- Vista ampla" };

        public ZipProcessor(ILogger<ZipProcessor> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task<PreviewData> ProcessZipAsync(string zipPath, UploadFormModel formData)
        {
            try
            {
                _logger.LogInformation($"Processing ZIP file: {zipPath}");
                
                var previewData = new PreviewData
                {
                    ProjectName = formData.NomeProjeto,
                    Folders = new List<FolderData>()
                };

                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Extract ZIP file
                    ZipFile.ExtractToDirectory(zipPath, tempDir);

                    // Find root folder
                    var extractedItems = Directory.GetFileSystemEntries(tempDir);
                    var rootPath = extractedItems.Length == 1 && Directory.Exists(extractedItems[0])
                        ? extractedItems[0]
                        : tempDir;

                    // Process folder structure
                    await ProcessDirectoryRecursive(rootPath, rootPath, previewData.Folders, 0);

                    // Count total images
                    previewData.TotalImages = previewData.Folders.Sum(f => f.Images.Count);

                    _logger.LogInformation($"Processed {previewData.TotalImages} images in {previewData.Folders.Count} folders");
                    
                    return previewData;
                }
                finally
                {
                    // Cleanup temp directory
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ZIP file");
                throw;
            }
        }

        public async Task<List<object>> ExtractAndOrganizeAsync(string zipPath)
        {
            var content = new List<object>();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Extract ZIP
                ZipFile.ExtractToDirectory(zipPath, tempDir);

                var extractedItems = Directory.GetFileSystemEntries(tempDir);
                var rootPath = extractedItems.Length == 1 && Directory.Exists(extractedItems[0])
                    ? extractedItems[0]
                    : tempDir;

                // Process structure
                await ProcessDirectoryForWordAsync(rootPath, rootPath, content, 0);

                return content;
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private async Task ProcessDirectoryRecursive(string currentPath, string rootPath, List<FolderData> folders, int level)
        {
            var relativePath = Path.GetRelativePath(rootPath, currentPath);
            
            if (relativePath == ".")
            {
                // Process subdirectories of root
                var subdirs = Directory.GetDirectories(currentPath)
                    .Select(Path.GetFileName)
                    .OrderBy(name => Array.IndexOf(ORDEM_PASTAS, name) == -1 ? int.MaxValue : Array.IndexOf(ORDEM_PASTAS, name))
                    .ThenBy(name => name);

                foreach (var subdir in subdirs)
                {
                    var subdirPath = Path.Combine(currentPath, subdir);
                    await ProcessDirectoryRecursive(subdirPath, rootPath, folders, level);
                }
                return;
            }

            var folderName = Path.GetFileName(currentPath);
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
            var actualLevel = pathParts.Length - 1;

            var folderData = new FolderData
            {
                Name = GenerateFolderTitle(folderName, actualLevel),
                Level = actualLevel,
                Images = new List<ImageData>()
            };

            // Process images in current folder
            var imageFiles = Directory.GetFiles(currentPath)
                .Where(f => IsImageFile(f))
                .OrderBy(f => f);

            foreach (var imagePath in imageFiles)
            {
                try
                {
                    var thumbnailPath = await GenerateThumbnailAsync(imagePath);
                    
                    folderData.Images.Add(new ImageData
                    {
                        Name = Path.GetFileName(imagePath),
                        Path = imagePath,
                        ThumbnailPath = thumbnailPath
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to process image: {imagePath}");
                }
            }

            if (folderData.Images.Count > 0 || Directory.GetDirectories(currentPath).Length > 0)
            {
                folders.Add(folderData);
            }

            // Process subdirectories
            var subdirectories = Directory.GetDirectories(currentPath)
                .OrderBy(d => Path.GetFileName(d));

            foreach (var subdir in subdirectories)
            {
                await ProcessDirectoryRecursive(subdir, rootPath, folders, level + 1);
            }
        }

        private async Task ProcessDirectoryForWordAsync(string currentPath, string rootPath, List<object> content, int level)
        {
            var relativePath = Path.GetRelativePath(rootPath, currentPath);
            
            if (relativePath == ".")
            {
                var subdirs = Directory.GetDirectories(currentPath)
                    .Select(Path.GetFileName)
                    .OrderBy(name => Array.IndexOf(ORDEM_PASTAS, name) == -1 ? int.MaxValue : Array.IndexOf(ORDEM_PASTAS, name))
                    .ThenBy(name => name);

                foreach (var subdir in subdirs)
                {
                    var subdirPath = Path.Combine(currentPath, subdir);
                    await ProcessDirectoryForWordAsync(subdirPath, rootPath, content, level);
                }
                return;
            }

            var folderName = Path.GetFileName(currentPath);
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
            var actualLevel = pathParts.Length - 1;

            // Add folder title
            content.Add(GenerateFolderTitle(folderName, actualLevel));

            // Process images in current folder
            var imageFiles = Directory.GetFiles(currentPath)
                .Where(f => IsImageFile(f))
                .OrderBy(f => f);

            foreach (var imagePath in imageFiles)
            {
                content.Add(new Dictionary<string, object> { { "imagem", imagePath } });
            }

            // Process subdirectories
            var subdirectories = Directory.GetDirectories(currentPath)
                .OrderBy(d => Path.GetFileName(d));

            foreach (var subdir in subdirectories)
            {
                await ProcessDirectoryForWordAsync(subdir, rootPath, content, level + 1);
            }
        }

        private string GenerateFolderTitle(string folderName, int level)
        {
            return level switch
            {
                0 => folderName,
                1 => $"»{folderName}",
                2 => $"»»{folderName}",
                _ => $"»»»{folderName}"
            };
        }

        private bool IsImageFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension is ".png" or ".jpg" or ".jpeg";
        }

        public async Task<string> GenerateThumbnailAsync(string imagePath, int width = 150, int height = 150)
        {
            try
            {
                var thumbnailDir = Path.Combine(_environment.WebRootPath, "thumbnails");
                Directory.CreateDirectory(thumbnailDir);

                var fileName = Path.GetFileNameWithoutExtension(imagePath);
                var extension = Path.GetExtension(imagePath);
                var uniqueId = Path.GetRandomFileName().Replace(".", "")[..8];
                var thumbnailName = $"{fileName}_{uniqueId}_thumb_{width}x{height}.jpg";
                var thumbnailPath = Path.Combine(thumbnailDir, thumbnailName);

                if (!File.Exists(thumbnailPath))
                {
                    using var image = await SixLabors.ImageSharp.Image.LoadAsync(imagePath);
                    
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Crop,
                        Position = AnchorPositionMode.Center
                    }));
                    
                    await image.SaveAsJpegAsync(thumbnailPath);
                }

                return thumbnailPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate thumbnail for: {imagePath}");
                return string.Empty; // Return empty string on error
            }
        }
    }
}