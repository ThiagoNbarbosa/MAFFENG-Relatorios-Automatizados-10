using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using MAFFENG.Models;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace MAFFENG.Services
{
    public class WordProcessor : IWordProcessor
    {
        private static readonly string[] PastasTextoNormal = { "- Detalhes", "- Vista ampla" };

        public async Task<int> GenerateReportAsync(string templatePath, List<object> content, Dictionary<string, string> placeholders, UploadFormModel formData, string outputPath)
        {
            int contadorImagens = 0;

            // Copy template to output location
            File.Copy(templatePath, outputPath, true);

            using var document = WordprocessingDocument.Open(outputPath, true);
            var mainPart = document.MainDocumentPart!;
            var doc = mainPart.Document;

            // Replace placeholders first
            ReplacePlaceholders(doc, formData, placeholders);

            // Find insertion point
            var paragraphs = doc.Body!.Elements<Paragraph>().ToList();
            var insertionIndex = FindInsertionPoint(paragraphs);

            // Process content with intelligent column layout
            var processedContent = await ProcessContentForColumnLayout(content);
            
            // Insert content in reverse order to maintain proper positioning
            processedContent.Reverse();

            foreach (var item in processedContent)
            {
                if (item is string folderTitle)
                {
                    InsertFolderTitle(doc, folderTitle, insertionIndex);
                }
                else if (item is Dictionary<string, object> dict)
                {
                    if (dict.ContainsKey("imagem"))
                    {
                        var imagePath = dict["imagem"].ToString()!;
                        if (await InsertImageAsync(document, imagePath, insertionIndex))
                        {
                            contadorImagens++;
                        }
                    }
                    else if (dict.ContainsKey("image_group"))
                    {
                        var imageGroup = (List<string>)dict["image_group"];
                        if (await InsertImageGroupAsync(document, imageGroup, insertionIndex))
                        {
                            contadorImagens += imageGroup.Count;
                        }
                    }
                    else if (dict.ContainsKey("quebra_pagina"))
                    {
                        InsertPageBreak(doc, insertionIndex);
                    }
                }
            }

            // Save the document
            doc.Save();
            return contadorImagens;
        }

        private void ReplacePlaceholders(Document doc, UploadFormModel formData, Dictionary<string, string> placeholders)
        {
            var placeholderData = new Dictionary<string, string>
            {
                ["{{prefixo_sb}}"] = formData.PrefixoAgencia ?? "",
                ["{{nome_ag}}"] = formData.NomeDependencia ?? "",
                ["{{uf}}"] = formData.UF ?? "",
                ["{{numero_contrato}}"] = formData.NumeroContrato ?? "",
                ["{{ordem_servico}}"] = formData.OrdemServico ?? "",
                ["{{data_elaboracao}}"] = formData.DataElaboracao?.ToString("dd/MM/yyyy") ?? "",
                ["{{tipo_atendimento}}"] = formData.TipoAtendimento ?? "",
                ["{{data_atendimento}}"] = formData.DataElaboracao?.ToString("dd/MM/yyyy") ?? "",
                ["{{endereco_dependencia}}"] = formData.EnderecoCompleto ?? "",
                ["{{responsavel_dependencia}}"] = formData.ResponsavelDependencia ?? "",
                ["{{responsavel_tecnico}}"] = formData.ResponsavelTecnico ?? "",
                ["{{elaborado_por}}"] = "Ygor Augusto Fernandes",
                ["{{empresa}}"] = "MAFFENG - Engenharia e Manutenção Profissional"
            };

            foreach (var paragraph in doc.Body!.Descendants<Paragraph>())
            {
                foreach (var run in paragraph.Elements<Run>())
                {
                    foreach (var text in run.Elements<Text>())
                    {
                        foreach (var (placeholder, value) in placeholderData)
                        {
                            if (text.Text.Contains(placeholder))
                            {
                                text.Text = text.Text.Replace(placeholder, value);
                            }
                        }
                    }
                }
            }
        }

        private int FindInsertionPoint(List<Paragraph> paragraphs)
        {
            for (int i = 0; i < paragraphs.Count; i++)
            {
                var text = GetParagraphText(paragraphs[i]);
                if (text.Contains("{{start_here}}"))
                {
                    // Clear the start_here marker
                    foreach (var run in paragraphs[i].Elements<Run>())
                    {
                        foreach (var textElement in run.Elements<Text>())
                        {
                            textElement.Text = textElement.Text.Replace("{{start_here}}", "");
                        }
                    }
                    return i;
                }
            }
            return paragraphs.Count - 1;
        }

        private string GetParagraphText(Paragraph paragraph)
        {
            return string.Join("", paragraph.Elements<Run>().SelectMany(r => r.Elements<Text>()).Select(t => t.Text));
        }

        private void InsertFolderTitle(Document doc, string folderTitle, int insertionIndex)
        {
            var tituloLimpo = folderTitle.Replace("»", "").Trim();
            if (tituloLimpo.StartsWith("- -"))
            {
                tituloLimpo = tituloLimpo[2..].Trim();
            }
            var titulo = tituloLimpo + ":";
            var nivel = folderTitle.Count(c => c == '»');

            var paragraph = new Paragraph();
            var run = new Run(new Text(titulo));

            // Apply styling based on folder type and hierarchy
            if (PastasTextoNormal.Any(pasta => titulo.Contains(pasta)))
            {
                run.RunProperties = new RunProperties(new Bold());
                paragraph.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Both });
            }
            else if (nivel == 0)
            {
                paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" });
            }
            else if (nivel == 1)
            {
                paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading2" });
            }
            else if (nivel == 2)
            {
                paragraph.ParagraphProperties = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading3" });
            }
            else
            {
                run.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "24" });
            }

            paragraph.Append(run);
            doc.Body!.InsertAt(paragraph, insertionIndex);
        }

        private async Task<bool> InsertImageAsync(WordprocessingDocument document, string imagePath, int insertionIndex)
        {
            if (!File.Exists(imagePath) || new FileInfo(imagePath).Length == 0)
                return false;

            try
            {
                var mainPart = document.MainDocumentPart!;
                var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

                // Process image with ImageSharp
                using var image = await Image.LoadAsync(imagePath);
                using var stream = new MemoryStream();
                
                // Resize image to maintain aspect ratio with 10cm height
                const int targetHeightCm = 10;
                const double cmToPixels = 28.35; // approximate conversion
                var targetHeightPixels = (int)(targetHeightCm * cmToPixels);
                var aspectRatio = (double)image.Width / image.Height;
                var targetWidthPixels = (int)(targetHeightPixels * aspectRatio);

                image.Mutate(x => x.Resize(targetWidthPixels, targetHeightPixels));
                await image.SaveAsJpegAsync(stream);
                stream.Position = 0;

                imagePart.FeedData(stream);
                var relationshipId = mainPart.GetIdOfPart(imagePart);

                // Create image element
                var paragraph = new Paragraph();
                var run = new Run();

                var drawing = new Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = targetWidthPixels * 9525, Cy = targetHeightPixels * 9525 },
                        new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                        new DW.DocProperties() { Id = 1U, Name = "Picture" },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks() { NoChangeAspect = true }),
                        new A.Graphic(
                            new A.GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties() { Id = 0U, Name = "Picture" },
                                        new PIC.NonVisualPictureDrawingProperties()),
                                    new PIC.BlipFill(
                                        new A.Blip() { Embed = relationshipId },
                                        new A.Stretch(new A.FillRectangle())),
                                    new PIC.ShapeProperties(
                                        new A.Transform2D(
                                            new A.Offset() { X = 0L, Y = 0L },
                                            new A.Extents() { Cx = targetWidthPixels * 9525, Cy = targetHeightPixels * 9525 }),
                                        new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                            ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    ) { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U }
                );

                run.Append(drawing);
                paragraph.Append(run);
                paragraph.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                document.MainDocumentPart!.Document.Body!.InsertAt(paragraph, insertionIndex);

                // Add spacing paragraph
                var spacingParagraph = new Paragraph();
                document.MainDocumentPart!.Document.Body!.InsertAt(spacingParagraph, insertionIndex);

                // Clean up temporary image file
                try
                {
                    File.Delete(imagePath);
                }
                catch
                {
                    // Ignore cleanup errors
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting image '{imagePath}': {ex.Message}");
                return false;
            }
        }

        private void InsertPageBreak(Document doc, int insertionIndex)
        {
            var paragraph = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
            doc.Body!.InsertAt(paragraph, insertionIndex);
        }

        private async Task<List<object>> ProcessContentForColumnLayout(List<object> content)
        {
            var processedContent = new List<object>();
            var currentImageGroup = new List<string>();
            
            foreach (var item in content)
            {
                if (item is Dictionary<string, object> dict && dict.ContainsKey("imagem"))
                {
                    var imagePath = dict["imagem"].ToString()!;
                    
                    // Check image dimensions to determine if it should be grouped
                    if (await IsSmallImageAsync(imagePath))
                    {
                        currentImageGroup.Add(imagePath);
                        
                        // If we have 3 small images, create a group
                        if (currentImageGroup.Count == 3)
                        {
                            processedContent.Add(new Dictionary<string, object>
                            {
                                ["image_group"] = new List<string>(currentImageGroup)
                            });
                            currentImageGroup.Clear();
                        }
                    }
                    else
                    {
                        // Flush any pending small images first
                        if (currentImageGroup.Count > 0)
                        {
                            processedContent.Add(new Dictionary<string, object>
                            {
                                ["image_group"] = new List<string>(currentImageGroup)
                            });
                            currentImageGroup.Clear();
                        }
                        
                        // Add large image individually
                        processedContent.Add(item);
                    }
                }
                else
                {
                    // Flush any pending small images before adding non-image content
                    if (currentImageGroup.Count > 0)
                    {
                        processedContent.Add(new Dictionary<string, object>
                        {
                            ["image_group"] = new List<string>(currentImageGroup)
                        });
                        currentImageGroup.Clear();
                    }
                    
                    processedContent.Add(item);
                }
            }
            
            // Flush any remaining small images
            if (currentImageGroup.Count > 0)
            {
                processedContent.Add(new Dictionary<string, object>
                {
                    ["image_group"] = new List<string>(currentImageGroup)
                });
            }
            
            return processedContent;
        }

        private async Task<bool> IsSmallImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                return false;

            try
            {
                using var image = await Image.LoadAsync(imagePath);
                
                // Calculate width in cm (approximate conversion: 1 cm ≈ 28.35 pixels at 72 DPI)
                const double pixelsToCm = 1.0 / 28.35;
                var widthCm = image.Width * pixelsToCm;
                
                // Consider small if width <= 7.5cm
                return widthCm <= 7.5;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> InsertImageGroupAsync(WordprocessingDocument document, List<string> imagePaths, int insertionIndex)
        {
            var validImages = new List<string>();
            
            // Filter valid images
            foreach (var imagePath in imagePaths)
            {
                if (File.Exists(imagePath) && new FileInfo(imagePath).Length > 0)
                {
                    validImages.Add(imagePath);
                }
            }
            
            if (validImages.Count == 0)
                return false;

            try
            {
                var mainPart = document.MainDocumentPart!;
                
                // Create table with one row and columns for each image
                var table = new Table();
                
                // Table properties
                var tableProperties = new TableProperties(
                    new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct },
                    new TableJustification() { Val = TableRowAlignmentValues.Center },
                    new TableBorders(
                        new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                        new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                        new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                        new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                        new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) },
                        new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.None) }
                    )
                );
                table.AppendChild(tableProperties);

                var tableRow = new TableRow();
                
                foreach (var imagePath in validImages)
                {
                    var cell = await CreateImageCellAsync(document, imagePath);
                    if (cell != null)
                    {
                        tableRow.AppendChild(cell);
                    }
                }
                
                table.AppendChild(tableRow);
                
                // Insert table into document
                document.MainDocumentPart!.Document.Body!.InsertAt(table, insertionIndex);
                
                // Add spacing paragraph after table
                var spacingParagraph = new Paragraph();
                document.MainDocumentPart!.Document.Body!.InsertAt(spacingParagraph, insertionIndex);
                
                // Clean up temporary image files
                foreach (var imagePath in validImages)
                {
                    try
                    {
                        File.Delete(imagePath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting image group: {ex.Message}");
                return false;
            }
        }

        private async Task<TableCell?> CreateImageCellAsync(WordprocessingDocument document, string imagePath)
        {
            try
            {
                var mainPart = document.MainDocumentPart!;
                var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

                // Process image with ImageSharp
                using var image = await Image.LoadAsync(imagePath);
                using var stream = new MemoryStream();
                
                // Resize image for column layout - smaller size (6cm height)
                const int targetHeightCm = 6;
                const double cmToPixels = 28.35;
                var targetHeightPixels = (int)(targetHeightCm * cmToPixels);
                var aspectRatio = (double)image.Width / image.Height;
                var targetWidthPixels = (int)(targetHeightPixels * aspectRatio);

                image.Mutate(x => x.Resize(targetWidthPixels, targetHeightPixels));
                await image.SaveAsJpegAsync(stream);
                stream.Position = 0;

                imagePart.FeedData(stream);
                var relationshipId = mainPart.GetIdOfPart(imagePart);

                var cell = new TableCell();
                
                // Cell properties for equal width distribution
                var cellProperties = new TableCellProperties(
                    new TableCellWidth() { Type = TableWidthUnitValues.Pct, Width = "3333" }, // 33.33% width
                    new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }
                );
                cell.AppendChild(cellProperties);

                var paragraph = new Paragraph();
                var run = new Run();

                var drawing = new Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = targetWidthPixels * 9525, Cy = targetHeightPixels * 9525 },
                        new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                        new DW.DocProperties() { Id = 1U, Name = "Picture" },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks() { NoChangeAspect = true }),
                        new A.Graphic(
                            new A.GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties() { Id = 0U, Name = "Picture" },
                                        new PIC.NonVisualPictureDrawingProperties()),
                                    new PIC.BlipFill(
                                        new A.Blip() { Embed = relationshipId },
                                        new A.Stretch(new A.FillRectangle())),
                                    new PIC.ShapeProperties(
                                        new A.Transform2D(
                                            new A.Offset() { X = 0L, Y = 0L },
                                            new A.Extents() { Cx = targetWidthPixels * 9525, Cy = targetHeightPixels * 9525 }),
                                        new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                            ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    ) { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U }
                );

                run.Append(drawing);
                paragraph.Append(run);
                paragraph.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                
                cell.Append(paragraph);
                return cell;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating image cell for '{imagePath}': {ex.Message}");
                return null;
            }
        }
    }
}