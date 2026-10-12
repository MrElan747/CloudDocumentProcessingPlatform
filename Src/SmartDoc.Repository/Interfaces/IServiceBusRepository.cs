using SmartDoc.Models.APIModels.DocumentsModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Repository.Interfaces
{
    public interface IServiceBusRepository
    {
        public Task SendDocumentProcessingMessageAsync(DocumentProcessingMessage message);
    }
}
