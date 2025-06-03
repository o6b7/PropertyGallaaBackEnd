using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyGalla.Data;
using PropertyGalla.DTOs.ViewRequestDTOs;
using PropertyGalla.Models;
using PropertyGalla.Services;
using System.Security.Claims;

namespace PropertyGalla.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ViewRequestsController : ControllerBase
    {
        private readonly PropertyGallaContext _context;
        private readonly IdGeneratorService _idGenerator;

        public ViewRequestsController(PropertyGallaContext context)
        {
            _context = context;
            _idGenerator = new IdGeneratorService(context);
        }

        // ✅ GET: api/ViewRequests
        [HttpGet]
        public async Task<IActionResult> GetViewRequests(
            [FromQuery] string? userId,
            [FromQuery] string? propertyId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var query = _context.ViewRequests
                .Include(vr => vr.Property)
                .AsQueryable();

            // Apply security filter
            if (!isAdmin)
            {
                query = query.Where(vr =>
                    vr.UserId == currentUserId ||
                    vr.Property.OwnerId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(vr => vr.UserId == userId);

            if (!string.IsNullOrWhiteSpace(propertyId))
                query = query.Where(vr => vr.PropertyId == propertyId);

            if (startDate.HasValue)
                query = query.Where(vr => vr.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(vr => vr.CreatedAt <= endDate.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var results = await query
                .OrderByDescending(vr => vr.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                currentPage = page,
                totalPages,
                totalCount,
                pageSize,
                results
            });
        }

        // ✅ POST: api/ViewRequests
        [HttpPost]
        public async Task<ActionResult<ViewRequest>> PostViewRequest([FromBody] CreateViewRequestDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (dto.UserId != currentUserId)
                return Forbid("You can only create view requests as yourself.");

            var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
            var propertyExists = await _context.Properties.AnyAsync(p => p.PropertyId == dto.PropertyId);

            if (!userExists || !propertyExists)
                return BadRequest(new { message = "Invalid UserId or PropertyId" });

            var existing = await _context.ViewRequests.FirstOrDefaultAsync(vr =>
                vr.UserId == dto.UserId &&
                vr.PropertyId == dto.PropertyId &&
                vr.Status == "pending");

            if (existing != null)
                return Conflict(new { message = "A pending request already exists." });

            var id = await _idGenerator.GenerateIdAsync("viewrequests");

            var viewRequest = new ViewRequest
            {
                ViewRequestId = id,
                UserId = dto.UserId,
                PropertyId = dto.PropertyId,
                Text = dto.Text,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.ViewRequests.Add(viewRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetViewRequests), new { userId = dto.UserId }, viewRequest);
        }

        // ✅ PATCH: api/ViewRequests/VRQ0001/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateViewRequestStatusDto dto)
        {
            var request = await _context.ViewRequests
                .Include(vr => vr.Property)
                .FirstOrDefaultAsync(vr => vr.ViewRequestId == id);

            if (request == null)
                return NotFound(new { message = "View request not found" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (request.Property.OwnerId != currentUserId)
                return Forbid("Only the owner of the property can update the request status.");

            if (dto.Status != "pending" && dto.Status != "handled" && dto.Status != "approved" && dto.Status != "rejected")
                return BadRequest(new { message = "Invalid status. Only pending, handled, approved, or rejected allowed." });

            request.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status updated to {dto.Status}" });
        }

        // ✅ DELETE: api/ViewRequests/VRQ0001 (can be restricted to user or admin if needed)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteViewRequest(string id)
        {
            var request = await _context.ViewRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = "View request not found" });

            _context.ViewRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
