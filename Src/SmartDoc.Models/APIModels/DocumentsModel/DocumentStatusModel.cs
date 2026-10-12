using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Models.APIModels.DocumentsModel
{
    public class DocumentStatusModel
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
    }
}
