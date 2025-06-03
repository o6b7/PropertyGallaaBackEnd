using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyGalla.Data;
using PropertyGalla.DTOs.ProprtyDTOs;
using PropertyGalla.Models;
using PropertyGalla.Services;

namespace PropertyGalla.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly PropertyGallaContext _context;
        private readonly IdGeneratorService _idGenerator;

        public PropertiesController(PropertyGallaContext context)
        {
            _context = context;
            _idGenerator = new IdGeneratorService(context);
        }

        // PUBLIC GET: /api/Properties?filters...
        [HttpGet]
        public async Task<IActionResult> GetProperties(
            [FromQuery] string? title = null,
            [FromQuery] string? state = null,
            [FromQuery] string? city = null,
            [FromQuery] int? rooms = null,
            [FromQuery] int? bathrooms = null,
            [FromQuery] int? parking = null,
            [FromQuery] double? minArea = null,
            [FromQuery] double? maxArea = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            var query = _context.Properties
                .Include(p => p.Images)
                .AsQueryable();

            // 🔍 Filtering
            if (!string.IsNullOrEmpty(title))
                query = query.Where(p => p.Title.Contains(title));
            if (!string.IsNullOrEmpty(state))
                query = query.Where(p => p.State.Contains(state));
            if (!string.IsNullOrEmpty(city))
                query = query.Where(p => p.City.Contains(city));
            if (rooms.HasValue)
                query = query.Where(p => p.Rooms == rooms);
            if (bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms == bathrooms);
            if (parking.HasValue)
                query = query.Where(p => p.Parking == parking);
            if (minArea.HasValue)
                query = query.Where(p => p.Area >= minArea);
            if (maxArea.HasValue)
                query = query.Where(p => p.Area <= maxArea);
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);
            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate);
            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate);

            // 📊 Pagination metadata
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 📦 Page results
            var properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = properties.Select(p => new GetPropertyDto
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Description = p.Description,
                Rooms = p.Rooms,
                Bathrooms = p.Bathrooms,
                Parking = p.Parking,
                Area = p.Area,
                State = p.State,
                City = p.City,
                Neighborhood = p.Neighborhood,
                Price = p.Price,
                OwnerId = p.OwnerId,
                Status = p.Status,
                Images = p.Images.Select(i => $"/api/Properties/image/{i.Id}").ToList()
            }).ToList();

            return Ok(new
            {
                currentPage = page,
                totalPages,
                totalCount,
                pageSize,
                properties = result
            });
        }

        // PUBLIC GET: /api/Properties/PRO0001
        [HttpGet("{id}")]
        public async Task<ActionResult<GetPropertyDto>> GetProperty(string id)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null)
                    return NotFound(new { message = $"Property with ID {id} not found." });

                return new GetPropertyDto
                {
                    PropertyId = property.PropertyId,
                    Title = property.Title,
                    Description = property.Description,
                    Rooms = property.Rooms,
                    Bathrooms = property.Bathrooms,
                    Parking = property.Parking,
                    Area = property.Area,
                    State = property.State,
                    City = property.City,
                    Neighborhood = property.Neighborhood,
                    Price = property.Price,
                    OwnerId = property.OwnerId,
                    Status = property.Status,
                    Images = property.Images.Select(i => $"/api/Properties/image/{i.Id}").ToList()
                };
            }
            catch (Exception ex)
            {
                // 👇 Debug log to console and return proper CORS-friendly error
                Console.WriteLine($"[ERROR GET /properties/{id}] {ex.Message}");

                HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return StatusCode(500, new
                {
                    message = "❌ Server error while retrieving property.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }


        [HttpGet("image/{imageId}")]
        public async Task<IActionResult> GetImage(int imageId)
        {
            var image = await _context.PropertyImages.FindAsync(imageId);
            if (image == null) return NotFound();
            return File(image.ImageData, image.ContentType);
        }

        [HttpPost("with-files")]
        public async Task<ActionResult<Property>> PostPropertyWithFiles([FromForm] CreatePropertyWithFilesDto dto)
        {
            try
            {
                // Log headers (including token if sent)
                Console.WriteLine("[DEBUG] Headers:");
                foreach (var header in Request.Headers)
                {
                    Console.WriteLine($"[DEBUG] Header: {header.Key} = {header.Value}");
                }

                // Log form data
                Console.WriteLine($"[DEBUG] Incoming Title: {dto.Title}");
                Console.WriteLine($"[DEBUG] Incoming Description: {dto.Description}");
                Console.WriteLine($"[DEBUG] Incoming OwnerId: {dto.OwnerId}");
                Console.WriteLine($"[DEBUG] Incoming State: {dto.State}, City: {dto.City}, Neighborhood: {dto.Neighborhood}");
                Console.WriteLine($"[DEBUG] Incoming Price: {dto.Price}, Area: {dto.Area}");
                Console.WriteLine($"[DEBUG] Image Count: {dto.Images?.Count ?? 0}");

                // Validation
                if (string.IsNullOrEmpty(dto.OwnerId))
                {
                    Console.WriteLine("[ERROR] OwnerId is missing");
                    return BadRequest(new { message = "OwnerId is required." });
                }

                // Generate ID
                var propertyId = await _idGenerator.GenerateIdAsync("properties");

                // Create entity
                var property = new Property
                {
                    PropertyId = propertyId,
                    Title = dto.Title,
                    Description = dto.Description,
                    Rooms = dto.Rooms,
                    Bathrooms = dto.Bathrooms,
                    Parking = dto.Parking,
                    Area = dto.Area,
                    State = dto.State,
                    City = dto.City,
                    Neighborhood = dto.Neighborhood,
                    Price = dto.Price,
                    OwnerId = dto.OwnerId,
                    Status = "available",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] Property added to DB.");

                // Add images
                if (dto.Images != null && dto.Images.Any())
                {
                    foreach (var file in dto.Images)
                    {
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        _context.PropertyImages.Add(new PropertyImage
                        {
                            PropertyId = propertyId,
                            ImageData = ms.ToArray(),
                            ContentType = file.ContentType
                        });
                    }
                    await _context.SaveChangesAsync();
                    Console.WriteLine("[DEBUG] Images saved.");
                }

                Console.WriteLine("[DEBUG] Property creation completed successfully.");
                return CreatedAtAction(nameof(GetProperty), new { id = property.PropertyId }, property);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Exception during property creation:");
                Console.WriteLine($"[ERROR] {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[ERROR] Inner: {ex.InnerException.Message}");

                return StatusCode(500, new
                {
                    message = "❌ Server error occurred.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }




        // PUT: /api/Properties/{id}
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutProperty(string id, [FromForm] UpdatePropertyDto dto)
        {
            if (id != dto.PropertyId)
                return BadRequest("Property ID mismatch.");

            var property = await _context.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.PropertyId == id);
            if (property == null)
                return NotFound();

            property.Title = dto.Title;
            property.Description = dto.Description;
            property.Rooms = dto.Rooms;
            property.Bathrooms = dto.Bathrooms;
            property.Parking = dto.Parking;
            property.Area = dto.Area;
            property.State = dto.State;
            property.City = dto.City;
            property.Neighborhood = dto.Neighborhood;
            property.Price = dto.Price;
            property.UpdatedAt = DateTime.Now;

            // Handle image removals
            var removeImageUrls = dto.RemoveImageUrls ?? new List<string>();
            var idsToRemove = removeImageUrls
                .Select(url => int.TryParse(url.Split("/").Last(), out var idVal) ? idVal : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            var toDelete = property.Images.Where(i => idsToRemove.Contains(i.Id)).ToList();
            _context.PropertyImages.RemoveRange(toDelete);

            // Add new images
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var file in dto.Images)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    _context.PropertyImages.Add(new PropertyImage
                    {
                        PropertyId = id,
                        ImageData = ms.ToArray(),
                        ContentType = file.ContentType
                    });
                }
            }

            var remainingImageCount = property.Images.Count - toDelete.Count + (dto.Images?.Count ?? 0);
            if (remainingImageCount == 0)
            {
                return BadRequest(new { message = "At least one image must remain after update." });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }



        // DELETE: Public (no auth required)
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteProperty(string id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found." });

            // Remove related SavedProperties
            var savedEntries = _context.SavedProperties.Where(sp => sp.PropertyId == id);
            _context.SavedProperties.RemoveRange(savedEntries);

            // Remove related ViewRequests
            var viewRequests = _context.ViewRequests.Where(vr => vr.PropertyId == id);
            _context.ViewRequests.RemoveRange(viewRequests);

            // Remove PropertyImages
            _context.PropertyImages.RemoveRange(property.Images);

            // Remove the property itself
            _context.Properties.Remove(property);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
