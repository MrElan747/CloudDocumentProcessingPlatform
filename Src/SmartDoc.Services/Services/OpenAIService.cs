using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using SmartDoc.Models.Enums;
using SmartDoc.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly ChatClient _chatClient;
        public OpenAIService(IConfiguration configuration)
        {
            var client = new AzureOpenAIClient(
              new Uri(configuration["AzureOpenAI:AIEndpoint"]),
              new AzureKeyCredential(configuration["AzureOpenAI:APIKey"]));

            _chatClient = client.GetChatClient(
                configuration["AzureOpenAI:ChatDeployment"]);
        }

        public async Task<DocumentTypeEnum> ClassifyAsync(string docText)
        {
            var supportedTypes = string.Join(", ", Enum.GetNames<DocumentTypeEnum>());

            var message = new ChatMessage[]
           {
                 new SystemChatMessage(
                 $"""
                    Classify the document into exactly one of these
                    supported types: {supportedTypes}.
                    
                    If the document does not match any supported type,
                    return Others.
                    
                    Return only the exact enum name.
                  """),

                new UserChatMessage($"Document content:\n{docText}")
           };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(message);

            if (!string.IsNullOrWhiteSpace(completion.Refusal))
            {
                throw new InvalidOperationException(
                    "Azure OpenAI declined the extraction request.");
            }

            if (completion.FinishReason != ChatFinishReason.Stop ||
                completion.Content.Count == 0)
            {
                throw new InvalidOperationException(
                    "Azure OpenAI returned an incomplete response.");
            }

            var result = completion.Content[0].Text.Trim();

            if (Enum.TryParse<DocumentTypeEnum>(
                    result, true, out var documentType)
                && Enum.IsDefined(documentType))
            {
                return documentType;
            }

            return DocumentTypeEnum.Others;
        }
    }
}
