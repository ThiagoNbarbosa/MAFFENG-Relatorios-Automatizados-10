using System.IO.Compression;
using MAFFENG.Models;

namespace MAFFENG.Services
{
    public class ZipProcessor : IZipProcessor
    {
        private static readonly string[] OrdemPastas = {
            "- Área externa",
            "- Área interna", 
            "- Segundo piso",
            "- Detalhes",
            "- Vista ampla"
        };

        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };

        public async Task<List<object>> ProcessZipAsync(string zipPath, UploadFormModel formData)
        {
            var conteudo = new List<object>();

            using var archive = ZipFile.OpenRead(zipPath);
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // Extract all files to temp directory
                archive.ExtractToDirectory(tempDir);

                // Find root folder
                var extractedItems = Directory.GetDirectories(tempDir).Concat(Directory.GetFiles(tempDir)).ToList();
                string pastaRaiz;

                if (extractedItems.Count == 1 && Directory.Exists(extractedItems[0]))
                {
                    pastaRaiz = extractedItems[0];
                }
                else
                {
                    pastaRaiz = tempDir;
                }

                // Process directory structure
                await ProcessDirectoryAsync(pastaRaiz, pastaRaiz, conteudo);
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            return conteudo;
        }

        private async Task ProcessDirectoryAsync(string currentPath, string rootPath, List<object> conteudo)
        {
            var allDirs = Directory.GetDirectories(currentPath);
            
            // Sort directories according to specified order
            var sortedDirs = allDirs.OrderBy(dir =>
            {
                var dirName = Path.GetFileName(dir);
                var index = Array.IndexOf(OrdemPastas, dirName);
                return index == -1 ? OrdemPastas.Length : index;
            }).ThenBy(Path.GetFileName);

            foreach (var dir in sortedDirs)
            {
                var relPath = Path.GetRelativePath(rootPath, dir);
                if (relPath == ".")
                    continue;

                var pathParts = relPath.Split(Path.DirectorySeparatorChar);
                var nivel = pathParts.Length - 1;
                var folderName = pathParts.Last();

                // Add folder title based on hierarchy level
                if (nivel == 0)
                {
                    conteudo.Add(folderName);
                }
                else if (nivel == 1)
                {
                    conteudo.Add($"»{folderName}");
                }
                else if (nivel == 2)
                {
                    conteudo.Add($"»»{folderName}");
                }
                else
                {
                    conteudo.Add($"»»»{folderName}");
                }

                // Process images in current folder
                var imageFiles = Directory.GetFiles(dir)
                    .Where(file => ImageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .OrderBy(File.GetCreationTime)
                    .ToList();

                foreach (var imagePath in imageFiles)
                {
                    // Copy image to permanent temp location for processing
                    var tempImagePath = Path.Combine(Path.GetTempPath(), $"temp_img_{Guid.NewGuid()}{Path.GetExtension(imagePath)}");
                    File.Copy(imagePath, tempImagePath, true);
                    
                    conteudo.Add(new Dictionary<string, object> { { "imagem", tempImagePath } });
                }

                // Add page break after each folder section
                if (imageFiles.Any())
                {
                    conteudo.Add(new Dictionary<string, object> { { "quebra_pagina", true } });
                }

                // Recursively process subdirectories
                await ProcessDirectoryAsync(dir, rootPath, conteudo);
            }
        }
    }
}