namespace PropertyGalla.DTOs.ProprtyDTOs
{
    public class GetPropertyDto
    {
        public string PropertyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public int Parking { get; set; }
        public double Area { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Neighborhood { get; set; }
        public decimal Price { get; set; }
        public string OwnerId { get; set; }
        public string Status { get; set; }
        public List<string> Images { get; set; } // These are URLs, not IFormFile. IFormFile is for uploads only.
    }

}
