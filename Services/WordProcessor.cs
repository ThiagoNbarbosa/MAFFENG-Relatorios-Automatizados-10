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

            // Insert content in reverse order to maintain proper positioning
            var reversedContent = content.ToList();
            reversedContent.Reverse();

            foreach (var item in reversedContent)
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
    }
}