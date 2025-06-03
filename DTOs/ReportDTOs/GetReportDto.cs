using System;

namespace PropertyGalla.DTOs.ReportDTOs
{
    public class GetReportDto
    {
        public string ReportId { get; set; }
        public string ReporterId { get; set; }
        public string PropertyId { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
