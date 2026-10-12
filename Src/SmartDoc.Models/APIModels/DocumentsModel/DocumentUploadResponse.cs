using SmartDoc.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Models.APIModels.DocumentsModel
{
    public class DocumentUploadResponse
    {
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DocumentTypeEnum DocumentType { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
