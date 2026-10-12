using SmartDoc.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Interfaces
{
    public interface IResumeExtractorService
    {
       public Task<ResumeExtractionResult> ExtractResumeAsync(string documentText, CancellationToken cancellationToken = default);
    }
}
