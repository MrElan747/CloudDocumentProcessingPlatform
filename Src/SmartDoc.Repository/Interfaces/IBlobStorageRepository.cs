using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Interfaces
{
    public interface IBlobStorageRepository
    {
        public Task<BlobUploadedResponse> UploadFileAsync(UploadDocumentRequest request, DocumentTypeEnum documentType);
        public Task<bool> DeleteBlobAsync(string fileName);
    }
}
