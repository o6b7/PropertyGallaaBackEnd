using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyGalla.Models
{
    public class Feedback
    {
        [Key]
        public string FeedbackId { get; set; }

        [Required]
        public string ReviewerId { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("ReviewerId")]
        public User Reviewer { get; set; }

        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }  // 1 to 5 stars

        public string? Comment { get; set; }  // Optional

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
