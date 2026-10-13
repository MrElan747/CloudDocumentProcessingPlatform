using Microsoft.AspNetCore.Http;
using SmartDoc.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Models.APIModels.DocumentsModel
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }
    }
}
