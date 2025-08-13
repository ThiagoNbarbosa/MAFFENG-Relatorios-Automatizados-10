using Microsoft.AspNetCore.Mvc;
using MAFFENG.Models;
using MAFFENG.Services;
using System.IO.Compression;
using System.Text.Json;

namespace MAFFENG.Controllers
{
    public class HomeController : Controller
    {
        private readonly IZipProcessor _zipProcessor;
        private readonly IWordProcessor _wordProcessor;
        private readonly IConfigManager _configManager;
        private readonly IWebHostEnvironment _environment;

        public HomeController(
            IZipProcessor zipProcessor, 
            IWordProcessor wordProcessor, 
            IConfigManager configManager,
            IWebHostEnvironment environment)
        {
            _zipProcessor = zipProcessor;
            _wordProcessor = wordProcessor;
            _configManager = configManager;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View(new UploadFormModel());
        }

        [HttpPost]
        public async Task<IActionResult> ProcessUpload(UploadFormModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Modelos = _configManager.GetAvailableModels();
                ViewBag.Estados = _configManager.GetBrazilianStates();
                return View("Index", model);
            }

            if (model.ArquivoZip == null || model.ArquivoZip.Length == 0)
            {
                ModelState.AddModelError("ArquivoZip", "Arquivo ZIP é obrigatório");
                ViewBag.Modelos = _configManager.GetAvailableModels();
                ViewBag.Estados = _configManager.GetBrazilianStates();
                return View("Index", model);
            }

            // Validate file extension
            if (!Path.GetExtension(model.ArquivoZip.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("ArquivoZip", "Apenas arquivos ZIP são permitidos");
                ViewBag.Modelos = _configManager.GetAvailableModels();
                ViewBag.Estados = _configManager.GetBrazilianStates();
                return View("Index", model);
            }

            try
            {
                // Save uploaded file
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var safeFileName = $"{timestamp}_{Path.GetFileName(model.ArquivoZip.FileName)}";
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
                var zipPath = Path.Combine(uploadsPath, safeFileName);

                using (var stream = new FileStream(zipPath, FileMode.Create))
                {
                    await model.ArquivoZip.CopyToAsync(stream);
                }

                // Validate ZIP file
                try
                {
                    using var archive = ZipFile.OpenRead(zipPath);
                    // Test if ZIP can be read
                }
                catch (InvalidDataException)
                {
                    System.IO.File.Delete(zipPath);
                    ModelState.AddModelError("ArquivoZip", "Arquivo ZIP corrompido ou inválido");
                    ViewBag.Modelos = _configManager.GetAvailableModels();
                    ViewBag.Estados = _configManager.GetBrazilianStates();
                    return View("Index", model);
                }

                // Check if model exists
                var modelPath = Path.Combine(_environment.ContentRootPath, "models", $"{model.ModeloSelecionado}.docx");
                if (!System.IO.File.Exists(modelPath))
                {
                    System.IO.File.Delete(zipPath);
                    ModelState.AddModelError("ModeloSelecionado", $"Modelo não encontrado: {model.ModeloSelecionado}");
                    ViewBag.Modelos = _configManager.GetAvailableModels();
                    ViewBag.Estados = _configManager.GetBrazilianStates();
                    return View("Index", model);
                }

                // Process ZIP file
                var previewData = await _zipProcessor.ProcessZipAsync(zipPath, model);

                // Store data in session for preview page
                HttpContext.Session.SetString("FormData", JsonSerializer.Serialize(model));
                HttpContext.Session.SetString("ZipPath", zipPath);
                HttpContext.Session.SetString("PreviewData", JsonSerializer.Serialize(previewData));
                HttpContext.Session.SetString("ModelPath", modelPath);

                return RedirectToAction("Preview");
            }
            catch (Exception ex)
            {
                // Log the detailed error for debugging
                Console.WriteLine($"ERRO DETALHADO ProcessUpload: {ex}");
                ModelState.AddModelError("", $"Erro ao processar arquivo: {ex.Message}");
                ViewBag.Modelos = _configManager.GetAvailableModels();
                ViewBag.Estados = _configManager.GetBrazilianStates();
                return View("Index", model);
            }
        }

        public IActionResult Preview()
        {
            var previewDataJson = HttpContext.Session.GetString("PreviewData");
            var formDataJson = HttpContext.Session.GetString("FormData");

            Console.WriteLine($"DEBUG Preview: PreviewData={!string.IsNullOrEmpty(previewDataJson)}, FormData={!string.IsNullOrEmpty(formDataJson)}");

            if (string.IsNullOrEmpty(previewDataJson) || string.IsNullOrEmpty(formDataJson))
            {
                TempData["Error"] = "Dados não encontrados. Por favor, faça o upload novamente.";
                return RedirectToAction("Index");
            }

            var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
            var formData = JsonSerializer.Deserialize<UploadFormModel>(formDataJson);

            // Convert PreviewData to PreviewModel format
            var previewItems = new List<PreviewItem>();

            foreach (var folder in previewData!.Folders)
            {
                var previewItem = new PreviewItem
                {
                    Type = "folder",
                    Title = folder.Name.Replace("»", "").Trim(),
                    OriginalTitle = folder.Name,
                    Level = folder.Level,
                    Images = new List<PreviewImage>()
                };

                foreach (var image in folder.Images)
                {
                    previewItem.Images.Add(new PreviewImage
                    {
                        Type = "image",
                        Name = image.Name,
                        Path = image.Path
                    });
                }

                previewItems.Add(previewItem);
            }

            var previewModel = new PreviewModel
            {
                PreviewItems = previewItems,
                FormData = formData!
            };

            return View(previewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport()
        {
            var formDataJson = HttpContext.Session.GetString("FormData");
            var zipPath = HttpContext.Session.GetString("ZipPath");
            var modelPath = HttpContext.Session.GetString("ModelPath");

            if (string.IsNullOrEmpty(formDataJson) || string.IsNullOrEmpty(zipPath) || string.IsNullOrEmpty(modelPath))
            {
                TempData["Error"] = "Dados não encontrados. Por favor, faça o upload novamente.";
                return RedirectToAction("Index");
            }

            try
            {
                var formData = JsonSerializer.Deserialize<UploadFormModel>(formDataJson)!;

                // Get customized order from form
                var customizedContent = new List<object>();
                var form = Request.Form;

                var i = 0;
                while (form.ContainsKey($"item_type_{i}"))
                {
                    var itemType = form[$"item_type_{i}"];

                    if (itemType == "folder")
                    {
                        var title = form[$"item_title_{i}"].ToString();
                        var level = int.Parse(form[$"item_level_{i}"]!);

                        // Reconstruct folder title with proper level markers
                        var folderTitle = level switch
                        {
                            0 => title,
                            1 => $"»{title}",
                            2 => $"»»{title}",
                            _ => $"»»»{title}"
                        };

                        customizedContent.Add(folderTitle);
                    }
                    else if (itemType == "image")
                    {
                        var imagePath = form[$"item_path_{i}"].ToString();
                        customizedContent.Add(new Dictionary<string, object> { { "imagem", imagePath } });
                    }

                    i++;
                }

                // Add page breaks between sections
                var finalContent = new List<object>();
                var currentFolderImages = new List<object>();

                foreach (var item in customizedContent)
                {
                    if (item is string)
                    {
                        // If we have accumulated images, add them and a page break
                        if (currentFolderImages.Any())
                        {
                            finalContent.AddRange(currentFolderImages);
                            finalContent.Add(new Dictionary<string, object> { { "quebra_pagina", true } });
                            currentFolderImages.Clear();
                        }
                        finalContent.Add(item);
                    }
                    else
                    {
                        currentFolderImages.Add(item);
                    }
                }

                // Add remaining images
                if (currentFolderImages.Any())
                {
                    finalContent.AddRange(currentFolderImages);
                    finalContent.Add(new Dictionary<string, object> { { "quebra_pagina", true } });
                }

                // Generate output filename
                var safeProjectName = string.Join("_", formData.NomeProjeto.Split(Path.GetInvalidFileNameChars()));
                var outputFilename = $"RELATÓRIO FOTOGRÁFICO - {safeProjectName} - LEVANTAMENTO PREVENTIVO.docx";
                var outputPath = Path.Combine(_environment.ContentRootPath, "output", outputFilename);

                // Generate Word document
                var placeholders = _configManager.GetPlaceholders();
                var numImagens = await _wordProcessor.GenerateReportAsync(modelPath, finalContent, placeholders, formData, outputPath);

                // Clean up temporary files
                if (System.IO.File.Exists(zipPath))
                {
                    System.IO.File.Delete(zipPath);
                }

                // Clear session data
                HttpContext.Session.Remove("FormData");
                HttpContext.Session.Remove("ZipPath");
                HttpContext.Session.Remove("PreviewData");
                HttpContext.Session.Remove("ModelPath");

                // Success message
                TempData["Success"] = $"Relatório gerado com sucesso! {numImagens} imagens inseridas.";

                ViewBag.Filename = outputFilename;
                ViewBag.NumImagens = numImagens;
                ViewBag.Projeto = formData.NomeProjeto;

                return View("Success");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao gerar relatório: {ex.Message}";
                return RedirectToAction("Preview");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Thumbnail(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
                {
                    return NotFound();
                }

                var thumbnailPath = await _zipProcessor.GenerateThumbnailAsync(imagePath);
                
                if (!System.IO.File.Exists(thumbnailPath))
                {
                    return NotFound();
                }

                var thumbnailBytes = await System.IO.File.ReadAllBytesAsync(thumbnailPath);
                return File(thumbnailBytes, "image/jpeg");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public IActionResult Download(string filename)
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, "output", filename);
                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "Arquivo não encontrado";
                    return RedirectToAction("Index");
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                return File(memory, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", filename);
            }
            catch
            {
                TempData["Error"] = "Erro ao baixar arquivo";
                return RedirectToAction("Index");
            }
        }
    }
}