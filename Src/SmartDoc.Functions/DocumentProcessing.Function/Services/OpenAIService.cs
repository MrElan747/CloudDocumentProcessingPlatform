using Azure;
using Azure.AI.OpenAI;
using DocumentProcessing.Function.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly ChatClient _chatClient;
        public OpenAIService(IConfiguration configuration)
        {
            var client = new AzureOpenAIClient(
              new Uri(configuration["AIEndpoint"]),
              new AzureKeyCredential(configuration["AIAPIKey"]));

            _chatClient = client.GetChatClient(
                configuration["ChatDeployment"]);
        }
        public async Task<string> GetChatCompletionAsync(string prompt, string docText, ChatCompletionOptions options, CancellationToken cancellationToken = default)
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
           new ChatMessage[]
           {
                new SystemChatMessage(prompt),
                new UserChatMessage(docText)
           },
           options,
           cancellationToken);

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

            return completion.Content[0].Text;
        }
    }
}
