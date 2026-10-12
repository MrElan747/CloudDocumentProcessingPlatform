using Microsoft.EntityFrameworkCore;
using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Models.EntityModels;
using SmartDoc.Models.Enums;
using SmartDoc.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Repository.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly CloudDocumentProcessingDbContext _dbcontext;
        public DocumentRepository(CloudDocumentProcessingDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<bool> InsertDocumentsDetails(Document doc, DocumentTypeEnum documentType)
        {
            doc.DocumentTypeId = await _dbcontext.DocumentType.Where(s => s.Name == documentType.ToString()).Select(c => c.Id).FirstOrDefaultAsync();

            await _dbcontext.Document.AddAsync(doc);
            int result = await _dbcontext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<DocumentStatusModel> GetDocumentStatusAsync(Guid id)
        {
            return await _dbcontext.Document.Where(x => x.CorrelationId == id)
                      .Select(s => new DocumentStatusModel
                      {
                          Status = s.Status,
                          UpdatedDate = s.ProcessedAt
                      }).FirstOrDefaultAsync() ?? new DocumentStatusModel();
        }
    }
}
