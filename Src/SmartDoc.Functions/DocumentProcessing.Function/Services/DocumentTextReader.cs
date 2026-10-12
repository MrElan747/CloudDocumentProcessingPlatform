using Azure;
using Azure.AI.OpenAI;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentProcessing.Function.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using SmartDoc.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DocumentProcessing.Function.Services
{
    public class DocumentTextReader : IDocumentTextReader
    {
        public async Task<string> ExtractAsync(Stream documentStream, string fileName, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(documentStream);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension is not (".pdf" or ".docx"))
            {
                throw new NotSupportedException(
                    "Only PDF and DOCX files are supported.");
            }

            // Copy from the current position without closing the input stream.
            using var buffer = new MemoryStream();
            await documentStream.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;

            StringBuilder text = new StringBuilder();

            if (extension == ".pdf")
            {
                using var pdf = PdfDocument.Open(buffer.ToArray());

                foreach (var page in pdf.GetPages())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    text.AppendLine(ContentOrderTextExtractor.GetText(page));
                }
            }
            else
            {
                using var word = WordprocessingDocument.Open(buffer, false);

                var mainPart = word.MainDocumentPart
                    ?? throw new InvalidDataException(
                        "The DOCX file has no main document part.");

                foreach (var header in mainPart.HeaderParts)
                {
                    AppendParagraphs(header.Header, text, cancellationToken);
                }

                AppendParagraphs(mainPart.Document.Body, text, cancellationToken);

                foreach (var footer in mainPart.FooterParts)
                {
                    AppendParagraphs(footer.Footer, text, cancellationToken);
                }
            }

            var result = text.ToString();

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException(
                    "No readable text found. The document may require OCR.");
            }

            return result;

        }

        private void AppendParagraphs(OpenXmlElement? root, StringBuilder text, CancellationToken cancellationToken = default)
        {
            if (root is null)
            {
                return;
            }

            // Includes paragraphs inside tables.
            foreach (var paragraph in root.Descendants<Paragraph>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(paragraph.InnerText))
                {
                    text.AppendLine(paragraph.InnerText);
                }
            }
        }
    }
}
