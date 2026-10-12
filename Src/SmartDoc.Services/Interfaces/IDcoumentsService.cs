using SmartDoc.Models.APIModels.DocumentsModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Interfaces
{
    public interface IDcoumentsService
    {
        public Task<DocumentUploadResponse> UploadFileAsync(UploadDocumentRequest request);

        public Task<DocumentStatusModel> GetDocumentStatusAsync(Guid id);
    }
}
