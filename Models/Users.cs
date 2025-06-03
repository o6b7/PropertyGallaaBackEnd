using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyGalla.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; }  // "user" or "admin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation collections
        public ICollection<Property> OwnedProperties { get; set; }

        [InverseProperty("Reviewer")]
        public ICollection<Feedback> GivenFeedbacks { get; set; }

        [InverseProperty("Owner")]
        public ICollection<Feedback> ReceivedFeedbacks { get; set; }


        public ICollection<ViewRequest> ViewRequests { get; set; }

        public ICollection<Report> SubmittedReports { get; set; }

        public ICollection<SavedProperty> SavedProperties { get; set; }
    }
}
