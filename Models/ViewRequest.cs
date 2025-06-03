using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyGalla.Models
{
    public class ViewRequest
    {
        [Key]
        public string ViewRequestId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string PropertyId { get; set; }

        [Required]
        public string Text { get; set; }

        public string Status { get; set; } = "pending"; // ✅ Default status

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("PropertyId")]
        public Property Property { get; set; }
    }
}
