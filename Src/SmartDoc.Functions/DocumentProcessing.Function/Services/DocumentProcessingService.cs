using DocumentProcessing.Function.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartDoc.Models;
using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Models.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Services
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly IBlobServices _blobService;
        private readonly CloudDocumentProcessingDbContext _dbContext;
        private readonly IDocumentTextReader _documentTextReader;
        private readonly IResumeExtractorService _resumeExtractorService;
        private readonly ILogger<DocumentProcessingService> _logger;

        public DocumentProcessingService(IBlobServices blobService, CloudDocumentProcessingDbContext context, IDocumentTextReader documentTextReader, IResumeExtractorService resumeExtractorService, ILogger<DocumentProcessingService> logger)
        {
            _blobService = blobService;
            _dbContext = context;
            _documentTextReader = documentTextReader;
            _resumeExtractorService = resumeExtractorService;
            _logger = logger;
        }

        public async Task ProcessAsync(DocumentProcessingMessage message, CancellationToken cancellationToken = default)
        {
            Document? doc = await _dbContext.Document.FirstOrDefaultAsync(x => x.CorrelationId == message.DocumentId);

            if (doc == null)
            {
                throw new InvalidOperationException($"Document not found: {message.DocumentId}");
            }

            bool alreadyProcessed = await _dbContext.Resume.AnyAsync(x => x.DocumentId == doc.Id, cancellationToken);

            if (alreadyProcessed)
            {
                if (doc.Status != "Processed")
                {
                    doc.Status = "Processed";
                    doc.ProcessedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Document {DocumentId} was already processed. Skipping duplicate.", doc.Id);

                return;
            }

            try
            {
                doc.Status = "Processing...";
                await _dbContext.SaveChangesAsync(cancellationToken);

                await using var documentStream = await _blobService.DownloadDocument(message, cancellationToken);

                string text = await _documentTextReader.ExtractAsync(documentStream, message.BlobName, cancellationToken);
                ResumeExtractionResult result = await _resumeExtractorService.ExtractResumeAsync(text, cancellationToken);

                Resume resume = new Resume
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    CandidateName = result.CandidateName,
                    Email = result.Email,
                    Phone = result.Mobile,
                    Summary = result.Summary,
                    TotalExperienceYears = result.TotalYearsOfExperience,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Resume.AddAsync(resume, cancellationToken);

                doc.Status = "Processed";
                doc.ProcessedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation( "Document {DocumentId} processed successfully.", doc.Id);
            }
            catch (Exception ex)
            {
                var pendingResumes = _dbContext.ChangeTracker
                                     .Entries<Resume>()
                                     .Where(e =>
                                         e.State == EntityState.Added &&
                                         e.Entity.DocumentId == doc.Id)
                                     .ToList();

                foreach (var entry in pendingResumes)
                {
                    entry.State = EntityState.Detached;
                }

                doc.Status = "Failed";
                doc.ProcessedAt = DateTime.UtcNow; 

                try
                {
                    await _dbContext.SaveChangesAsync(CancellationToken.None);
                }
                catch (Exception statusException)
                {
                    _logger.LogError(statusException, "Could not update failure status for document {DocumentId}.", doc.Id);
                }

                _logger.LogError(ex, "Document processing failed for {DocumentId}.", doc.Id);

                throw;
            }
        }
    }
}
