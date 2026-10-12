using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Models.EntityModels;
using SmartDoc.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Repository.Interfaces
{
    public interface IDocumentRepository
    {
        public Task<bool> InsertDocumentsDetails(Document doc, DocumentTypeEnum documentType);

        public Task<DocumentStatusModel> GetDocumentStatusAsync(Guid id);
    }
}
