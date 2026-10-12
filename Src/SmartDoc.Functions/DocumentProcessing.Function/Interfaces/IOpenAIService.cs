using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Interfaces
{
    public interface IOpenAIService
    {
        public Task<string> GetChatCompletionAsync(string prompt, string docText, ChatCompletionOptions options, CancellationToken cancellationToken = default);
    }
}
