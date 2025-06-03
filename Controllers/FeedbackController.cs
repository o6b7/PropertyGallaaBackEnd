using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyGalla.Data;
using PropertyGalla.DTOs.FeedbackDTOs;
using PropertyGalla.Models;
using PropertyGalla.Services;
using System.Security.Claims;

namespace PropertyGalla.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly PropertyGallaContext _context;
        private readonly IdGeneratorService _idGenerator;

        public FeedbackController(PropertyGallaContext context)
        {
            _context = context;
            _idGenerator = new IdGeneratorService(context);
        }

        // ✅ GET: api/Feedback (Only logged-in users)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<GetFeedbackDto>>> GetFeedbacks(
            [FromQuery] string? ownerId = null,
            [FromQuery] string? reviewerId = null,
            [FromQuery] int? rating = null,
            [FromQuery] int? minRating = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            var query = _context.Feedbacks
                .Include(f => f.Reviewer)
                .Include(f => f.Owner)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ownerId))
                query = query.Where(f => f.OwnerId == ownerId);

            if (!string.IsNullOrWhiteSpace(reviewerId))
                query = query.Where(f => f.ReviewerId == reviewerId);

            if (rating.HasValue)
                query = query.Where(f => f.Rating == rating);
            else if (minRating.HasValue)
                query = query.Where(f => f.Rating >= minRating);

            var total = await query.CountAsync();

            var feedbacks = await query
                .OrderByDescending(f => f.SubmittedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)total / pageSize),
                data = feedbacks.Select(f => new GetFeedbackDto
                {
                    FeedbackId = f.FeedbackId,
                    ReviewerId = f.ReviewerId,
                    ReviewerName = f.Reviewer.Name,
                    OwnerId = f.OwnerId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    SubmittedAt = f.SubmittedAt
                }).ToList()
            });
        }

        // ✅ POST: api/Feedback (Authenticated users only)
        [HttpPost]
        public async Task<ActionResult<GetFeedbackDto>> PostFeedback(CreateFeedbackDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (dto.ReviewerId != currentUserId)
                return Forbid("You can only submit feedback as yourself.");

            if (dto.ReviewerId == dto.OwnerId)
                return BadRequest(new { message = "You cannot rate yourself." });

            var ownerExists = await _context.Users.AnyAsync(u => u.UserId == dto.OwnerId);
            var reviewerExists = await _context.Users.AnyAsync(u => u.UserId == dto.ReviewerId);

            if (!ownerExists || !reviewerExists)
                return BadRequest(new { message = "OwnerId or ReviewerId does not exist" });

            bool alreadyRated = await _context.Feedbacks.AnyAsync(f =>
                f.ReviewerId == dto.ReviewerId && f.OwnerId == dto.OwnerId);

            if (alreadyRated)
                return Conflict(new { message = "You have already rated this owner." });

            var feedbackId = await _idGenerator.GenerateIdAsync("feedback");

            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                ReviewerId = dto.ReviewerId,
                OwnerId = dto.OwnerId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                SubmittedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            var reviewerName = (await _context.Users.FindAsync(dto.ReviewerId))?.Name ?? "N/A";

            return CreatedAtAction(nameof(GetFeedbacks), new { ownerId = dto.OwnerId }, new GetFeedbackDto
            {
                FeedbackId = feedbackId,
                ReviewerId = dto.ReviewerId,
                ReviewerName = reviewerName,
                OwnerId = dto.OwnerId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                SubmittedAt = feedback.SubmittedAt
            });
        }

        // ✅ DELETE: api/Feedback/FED0001 (Reviewer or Admin)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            if (feedback.ReviewerId != currentUserId && !isAdmin)
                return Forbid("You can only delete your own feedback unless you're an admin.");

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
