using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Models.APIModels.DocumentsModel
{
    public class DocumentProcessingMessage
    {
        public Guid DocumentId { get; set; }
        public string BlobContainer { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public int DocumentTypeId { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }
}
