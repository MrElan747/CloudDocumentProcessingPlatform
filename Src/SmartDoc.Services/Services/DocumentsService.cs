using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Models.EntityModels;
using SmartDoc.Models.Enums;
using SmartDoc.Repository.Interfaces;
using SmartDoc.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Services.Services
{
    public class DocumentsService : IDcoumentsService
    {
        private readonly IBlobStorageRepository _blobStorageRepo;
        private readonly IDocumentRepository _documentRepo;
        private readonly IServiceBusRepository _serviceBusRepo;
        public DocumentsService(IBlobStorageRepository blobStorageRepo, IDocumentRepository documentRepo, IServiceBusRepository serviceBusRepo)
        {
            _blobStorageRepo = blobStorageRepo;
            _documentRepo = documentRepo;
            _serviceBusRepo = serviceBusRepo;
        }

        public async Task<DocumentUploadResponse> UploadFileAsync(UploadDocumentRequest request)
        {

            BlobUploadedResponse res = await _blobStorageRepo.UploadFileAsync(request);

            Document doc = new Document()
            {
                Id = Guid.NewGuid(),
                OriginalFilename = request.File.FileName,
                BlobContiner = res.ContainerName,
                BlobPath = res.BlobPath,
                ContentType = res.ContentType,
                FileSize = res.Size,
                Status = res.Status,
                CorrelationId = res.DocumentId,
                CreatedAt = DateTime.Now
            };

            bool status = await _documentRepo.InsertDocumentsDetails(doc, res.DocumentType);

            if (status)
            {
                DocumentProcessingMessage documentProcessing = new DocumentProcessingMessage()
                {
                    DocumentId = res.DocumentId,
                    BlobContainer = res.ContainerName,
                    BlobName = res.BlobPath,
                    DocumentTypeId = (int)res.DocumentType,
                    CorrelationId = res.DocumentId.ToString()
                };
                await _serviceBusRepo.SendDocumentProcessingMessageAsync(documentProcessing);
                return new DocumentUploadResponse() { DocumentId = res.DocumentId, DocumentType = res.DocumentType, FileName = res.FileName, Status = res.Status };
            }
            else
            {
                await _blobStorageRepo.DeleteBlobAsync(res.FileName);

                return new DocumentUploadResponse() { DocumentId = Guid.Empty, DocumentType = 0, FileName = "", Status = "Blob Not Uploaded." };
            }

        }

        public async Task<DocumentStatusModel> GetDocumentStatusAsync(Guid id)
        {
            return await _documentRepo.GetDocumentStatusAsync(id);
        }
    }
}
