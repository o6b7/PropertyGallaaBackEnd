using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PropertyGalla.DTOs.ProprtyDTOs
{
    public class CreatePropertyWithFilesDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Rooms count is required")]
        public int Rooms { get; set; }

        [Required(ErrorMessage = "Bathrooms count is required")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "Parking spaces count is required")]
        public int Parking { get; set; }

        [Required(ErrorMessage = "Area is required")]
        public double Area { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Neighborhood is required")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Owner ID is required")]
        public string OwnerId { get; set; }

        [Required(ErrorMessage = "At least one image is required")]
        public List<IFormFile> Images { get; set; }
    }
}