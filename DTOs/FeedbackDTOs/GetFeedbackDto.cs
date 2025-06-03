using System;

namespace PropertyGalla.DTOs.FeedbackDTOs
{
    public class GetFeedbackDto
    {
        public string FeedbackId { get; set; }
        public string ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string OwnerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
