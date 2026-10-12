using DocumentProcessing.Function.Interfaces;
using OpenAI.Chat;
using SmartDoc.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DocumentProcessing.Function.Services
{
    public class ResumeExtractorService : IResumeExtractorService
    {
        private readonly string _resumePrompt;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        private const string ResumeSchema = """
        {
          "type": "object",
          "properties": {
            "candidateName": { "type": ["string", "null"] },
            "email": { "type": ["string", "null"] },
            "mobile": { "type": ["string", "null"] },
            "summary": { "type": ["string", "null"] },
            "totalYearsOfExperience": { "type": ["number", "null"] }
          },
          "required": [
            "candidateName",
            "email",
            "mobile",
            "summary",
            "totalYearsOfExperience"
          ],
          "additionalProperties": false
        }
        """;
        private readonly IOpenAIService _openAIService;
        public ResumeExtractorService(IOpenAIService openAIService)
        {

            var promptPath = Path.Combine(
                AppContext.BaseDirectory,
                "Prompts",
                "ResumeExtraction.txt");

            _resumePrompt = File.ReadAllText(promptPath);

            if (string.IsNullOrWhiteSpace(_resumePrompt))
            {
                throw new InvalidOperationException(
                    "The resume extraction prompt is empty.");
            }

            _openAIService = openAIService;
        }

        public async Task<ResumeExtractionResult> ExtractResumeAsync(string documentText, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentText);

            // Application limit; not a model token-limit calculation.
            if (documentText.Length > 60_000)
            {
                throw new ArgumentException(
                    "Document text exceeds the supported size.",
                    nameof(documentText));
            }

            var prompt = _resumePrompt.Replace(
                "{{AS_OF_DATE}}",
                DateTime.UtcNow.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture),
                StringComparison.Ordinal);

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "resume_extraction",
                    jsonSchema: BinaryData.FromString(ResumeSchema),
                    jsonSchemaIsStrict: true)
            };

            string aiRes = await _openAIService.GetChatCompletionAsync(prompt, documentText, options, cancellationToken);

            return JsonSerializer.Deserialize<ResumeExtractionResult>(
                aiRes,
                JsonOptions)
                ?? throw new InvalidOperationException(
                    "Azure OpenAI returned an empty result.");
        }

    }
}
