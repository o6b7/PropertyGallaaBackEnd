namespace PropertyGalla.DTOs.FeedbackDTOs
{
    public class CreateFeedbackDto
    {
        public string ReviewerId { get; set; }
        public string OwnerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
