using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using SmartDoc.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UglyToad.PdfPig;

namespace SmartDoc.Services.Services
{
    public class DocumentTextExtractor : IDocumentTextExtractor
    {
        public async Task<string> ExtractTextAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            using var stream = file.OpenReadStream();

            switch (extension)
            {
                case ".pdf":
                    {
                        using var pdf = PdfDocument.Open(stream);

                        return string.Join(
                            "\n",
                            pdf.GetPages().Select(page => page.Text)
                        );
                    }

                case ".docx":
                    {
                        using var word = WordprocessingDocument.Open(
                            stream, false);

                        return word.MainDocumentPart?
                            .Document.Body?
                            .InnerText ?? string.Empty;
                    }

                case ".txt":
                    {
                        using var reader = new StreamReader(stream);

                        return await reader.ReadToEndAsync();
                    }

                default:
                    throw new NotSupportedException(
                        "Unsupported document format.");
            }
        }
    }
}
