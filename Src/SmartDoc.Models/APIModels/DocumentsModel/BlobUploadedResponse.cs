using SmartDoc.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Models.APIModels.DocumentsModel
{
    public class BlobUploadedResponse
    {
        public Guid DocumentId { get; set; }
        public string FileName {  get; set; }
        public DocumentTypeEnum DocumentType { get; set; }
        public string ContainerName { get; set; }
        public string BlobPath { get; set; }
        public string Status {  get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }
}
