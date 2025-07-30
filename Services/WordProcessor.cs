using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MAFFENG.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MAFFENG.Services
{
    public interface IWordProcessor
    {
        Task<int> GenerateReportAsync(string modelPath, List<object> content, Dictionary<string, string> placeholders, UploadFormModel formData, string outputPath);
    }

    public class WordProcessor : IWordProcessor
    {
        private readonly ILogger<WordProcessor> _logger;

        public WordProcessor(ILogger<WordProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<int> GenerateReportAsync(string modelPath, List<object> content, Dictionary<string, string> placeholders, UploadFormModel formData, string outputPath)
        {
            try
            {
                _logger.LogInformation($"Starting Word document generation: {outputPath}");
                
                // Create output directory if it doesn't exist
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Copy template to output location
                File.Copy(modelPath, outputPath, true);

                int imageCount = 0;

                using var wordDocument = WordprocessingDocument.Open(outputPath, true);
                var body = wordDocument.MainDocumentPart?.Document.Body;

                if (body == null)
                {
                    throw new InvalidOperationException("Document body not found");
                }

                // Replace placeholders with form data
                await ReplacePlaceholdersAsync(wordDocument, formData);

                // Process content and insert images
                foreach (var item in content)
                {
                    if (item is string title)
                    {
                        // Insert folder title as heading
                        InsertHeading(body, title);
                    }
                    else if (item is Dictionary<string, object> dict)
                    {
                        if (dict.ContainsKey("imagem"))
                        {
                            var imagePath = dict["imagem"].ToString();
                            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                            {
                                await InsertImageAsync(wordDocument, body, imagePath);
                                imageCount++;
                                
                                // Add line break after image
                                body.AppendChild(new Paragraph(new Run(new Break())));
                            }
                        }
                        else if (dict.ContainsKey("quebra_pagina"))
                        {
                            // Insert page break
                            InsertPageBreak(body);
                        }
                    }
                }

                wordDocument.Save();
                _logger.LogInformation($"Document generated successfully with {imageCount} images");
                
                return imageCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Word document");
                throw;
            }
        }

        private async Task ReplacePlaceholdersAsync(WordprocessingDocument wordDocument, UploadFormModel formData)
        {
            var body = wordDocument.MainDocumentPart?.Document.Body;
            if (body == null) return;

            var placeholders = new Dictionary<string, string>
            {
                { "{{nome_projeto}}", formData.NomeProjeto },
                { "{{numero_contrato}}", formData.NumeroContrato },
                { "{{ordem_servico}}", formData.OrdemServico },
                { "{{data_elaboracao}}", formData.DataElaboracao.ToString("dd/MM/yyyy") },
                { "{{data_atendimento}}", formData.DataAtendimento.ToString("dd/MM/yyyy") },
                { "{{tipo_atendimento}}", formData.TipoAtendimento },
                { "{{prefixo_sb}}", formData.PrefixoSB },
                { "{{nome_ag}}", formData.NomeAG },
                { "{{uf}}", formData.UF },
                { "{{endereco_dependencia}}", formData.EnderecoCompleto },
                { "{{responsavel_dependencia}}", formData.ResponsavelDependencia },
                { "{{responsavel_tecnico}}", formData.ResponsavelTecnico },
                { "{{metodologia_trabalho}}", formData.MetodologiaTrabalho },
                { "{{objetivo_trabalho}}", formData.ObjetivoTrabalho }
            };

            foreach (var text in body.Descendants<Text>())
            {
                foreach (var placeholder in placeholders)
                {
                    if (text.Text.Contains(placeholder.Key))
                    {
                        text.Text = text.Text.Replace(placeholder.Key, placeholder.Value ?? "");
                    }
                }
            }
        }

        private void InsertHeading(Body body, string title)
        {
            var level = title.Count(c => c == '»');
            var cleanTitle = title.Replace("»", "").Trim();
            
            if (cleanTitle.StartsWith("- -"))
            {
                cleanTitle = cleanTitle[2..].Trim();
            }

            var paragraph = new Paragraph();
            var run = new Run(new Text(cleanTitle));
            
            // Apply heading style based on level
            var paragraphProperties = new ParagraphProperties();
            var paragraphStyleId = new ParagraphStyleId() { Val = level switch
            {
                0 => "Heading1",
                1 => "Heading2", 
                2 => "Heading3",
                _ => "Heading4"
            }};
            
            paragraphProperties.AppendChild(paragraphStyleId);
            paragraph.PrependChild(paragraphProperties);
            paragraph.AppendChild(run);
            
            body.AppendChild(paragraph);
        }

        private async Task InsertImageAsync(WordprocessingDocument wordDocument, Body body, string imagePath)
        {
            try
            {
                var imagePart = wordDocument.MainDocumentPart?.AddImagePart(ImagePartType.Jpeg);
                if (imagePart == null) return;

                // Process and resize image
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(imagePath);
                
                // Convert to JPEG and resize if needed
                using var memoryStream = new MemoryStream();
                
                // Resize large images to fit page width (max 15cm = 567 pixels at 96 DPI)
                if (image.Width > 567)
                {
                    image.Mutate(x => x.Resize(567, 0));
                }
                
                await image.SaveAsJpegAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                
                imagePart.FeedData(memoryStream);
                var relationshipId = wordDocument.MainDocumentPart.GetIdOfPart(imagePart);

                // Create image element
                var element = new Drawing(
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() 
                        { 
                            Cx = image.Width * 9525L, // Convert pixels to EMUs
                            Cy = image.Height * 9525L 
                        },
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties()
                        {
                            Id = 1U,
                            Name = Path.GetFileName(imagePath)
                        },
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                            new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks() { NoChangeAspect = true }),
                        new DocumentFormat.OpenXml.Drawing.Graphic(
                            new DocumentFormat.OpenXml.Drawing.GraphicData(
                                new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties()
                                        {
                                            Id = 0U,
                                            Name = Path.GetFileName(imagePath)
                                        },
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                                    new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                        new DocumentFormat.OpenXml.Drawing.Blip(
                                            new DocumentFormat.OpenXml.Drawing.BlipExtensionList(
                                                new DocumentFormat.OpenXml.Drawing.BlipExtension()
                                                {
                                                    Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                }))
                                        {
                                            Embed = relationshipId
                                        },
                                        new DocumentFormat.OpenXml.Drawing.Stretch(
                                            new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                                    new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                        new DocumentFormat.OpenXml.Drawing.Transform2D(
                                            new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                            new DocumentFormat.OpenXml.Drawing.Extents() 
                                            { 
                                                Cx = image.Width * 9525L, 
                                                Cy = image.Height * 9525L 
                                            }),
                                        new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                            new DocumentFormat.OpenXml.Drawing.AdjustValueList())
                                        { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle })))
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
                    { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U });

                var paragraph = new Paragraph(new Run(element));
                body.AppendChild(paragraph);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inserting image: {imagePath}");
            }
        }

        private void InsertPageBreak(Body body)
        {
            var paragraph = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
            body.AppendChild(paragraph);
        }
    }
}