using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyGalla.Models
{
    public class Report
    {
        [Key]
        public string ReportId { get; set; }

        [Required]
        public string ReporterId { get; set; }  // FK → Users.UserId

        [Required]
        public string PropertyId { get; set; }  // FK → Properties.PropertyId

        [Required]
        public string Reason { get; set; }

        public string Status { get; set; } = "pending";  // pending, reviewed, dismissed

        public string? Note { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ReporterId")]
        public User Reporter { get; set; }

        [ForeignKey("PropertyId")]
        public Property Property { get; set; }
    }
}
