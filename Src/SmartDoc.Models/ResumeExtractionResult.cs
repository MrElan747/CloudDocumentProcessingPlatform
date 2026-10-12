using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDoc.Models
{
    public class ResumeExtractionResult
    {
        public string CandidateName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public decimal TotalYearsOfExperience { get; set; }
    }
}
