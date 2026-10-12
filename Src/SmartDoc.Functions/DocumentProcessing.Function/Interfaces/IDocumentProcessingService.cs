using SmartDoc.Models.APIModels.DocumentsModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Interfaces
{
    public interface IDocumentProcessingService
    {
        public Task ProcessAsync(DocumentProcessingMessage message, CancellationToken cancellationToken = default);
    }
}
