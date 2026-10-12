using SmartDoc.Models.APIModels.DocumentsModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Interfaces
{
    public interface IBlobServices
    {
        public Task<Stream> DownloadDocument(DocumentProcessingMessage message, CancellationToken cancellationToken = default);
    }
}
