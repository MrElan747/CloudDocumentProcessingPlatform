using SmartDoc.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Interfaces
{
    public interface IOpenAIService
    {
        public Task<DocumentTypeEnum> ClassifyAsync(string docText);
    }
}
