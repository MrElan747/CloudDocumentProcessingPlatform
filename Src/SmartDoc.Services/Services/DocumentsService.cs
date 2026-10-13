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
        private readonly IDocumentTextExtractor _documentTextExtractor;
        private readonly IOpenAIService _openAIService;
        public DocumentsService(IBlobStorageRepository blobStorageRepo, IDocumentRepository documentRepo, IServiceBusRepository serviceBusRepo, IDocumentTextExtractor documentTextExtractor, IOpenAIService openAIService)
        {
            _blobStorageRepo = blobStorageRepo;
            _documentRepo = documentRepo;
            _serviceBusRepo = serviceBusRepo;
            _documentTextExtractor = documentTextExtractor;
            _openAIService = openAIService;
        }

        public async Task<DocumentUploadResponse> UploadFileAsync(UploadDocumentRequest request)
        {

            string docText = await _documentTextExtractor.ExtractTextAsync(request.File);

            DocumentTypeEnum documentType = await _openAIService.ClassifyAsync(docText);

            BlobUploadedResponse res = await _blobStorageRepo.UploadFileAsync(request, documentType);

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

            bool status = await _documentRepo.InsertDocumentsDetails(doc, documentType);

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
                return new DocumentUploadResponse() { DocumentId = res.DocumentId, DocumentType = documentType, FileName = res.FileName, Status = res.Status };
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
