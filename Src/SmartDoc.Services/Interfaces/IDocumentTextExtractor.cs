using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Interfaces
{
    public interface IDocumentTextExtractor
    {
        public Task<string> ExtractTextAsync(IFormFile file);
    }
}
