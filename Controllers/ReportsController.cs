using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyGalla.Data;
using PropertyGalla.DTOs.ReportDTOs;
using PropertyGalla.Models;
using PropertyGalla.Services;
using System.Security.Claims;

namespace PropertyGalla.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly PropertyGallaContext _context;
        private readonly IdGeneratorService _idGenerator;

        public ReportsController(PropertyGallaContext context)
        {
            _context = context;
            _idGenerator = new IdGeneratorService(context);
        }

        // ✅ GET: api/Reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetReportDto>>> GetReports(
            [FromQuery] string? reporterId = null,
            [FromQuery] string? propertyId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var query = _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Property)
                .AsQueryable();

            if (!isAdmin)
            {
                var ownedPropertyIds = await _context.Properties
                    .Where(p => p.OwnerId == currentUserId)
                    .Select(p => p.PropertyId)
                    .ToListAsync();

                query = query.Where(r =>
                    r.ReporterId == currentUserId ||
                    ownedPropertyIds.Contains(r.PropertyId));
            }

            if (!string.IsNullOrEmpty(reporterId))
                query = query.Where(r => r.ReporterId == reporterId);
            if (!string.IsNullOrEmpty(propertyId))
                query = query.Where(r => r.PropertyId == propertyId);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);
            if (startDate.HasValue)
                query = query.Where(r => r.CreatedAt >= startDate);
            if (endDate.HasValue)
                query = query.Where(r => r.CreatedAt <= endDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GetReportDto
                {
                    ReportId = r.ReportId,
                    ReporterId = r.ReporterId,
                    PropertyId = r.PropertyId,
                    Reason = r.Reason,
                    Status = r.Status,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                currentPage = page,
                totalPages,
                pageSize,
                totalCount,
                reports
            });
        }

        // ✅ GET: api/Reports/REP0001
        [HttpGet("{id}")]
        public async Task<ActionResult<GetReportDto>> GetReport(string id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound(new { message = "Report not found" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("admin");

            var ownsProperty = await _context.Properties
                .AnyAsync(p => p.PropertyId == report.PropertyId && p.OwnerId == currentUserId);

            if (!isAdmin && report.ReporterId != currentUserId && !ownsProperty)
                return Forbid("You can only view your own reports or reports made about your properties.");

            return new GetReportDto
            {
                ReportId = report.ReportId,
                ReporterId = report.ReporterId,
                PropertyId = report.PropertyId,
                Reason = report.Reason,
                Status = report.Status,
                Note = report.Note,
                CreatedAt = report.CreatedAt
            };
        }

        // ✅ POST: api/Reports
        [HttpPost]
        public async Task<ActionResult<GetReportDto>> PostReport(CreateReportDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != dto.ReporterId)
                return Forbid("You can only report as yourself.");

            var reporterExists = await _context.Users.AnyAsync(u => u.UserId == dto.ReporterId);
            var propertyExists = await _context.Properties.AnyAsync(p => p.PropertyId == dto.PropertyId);

            if (!reporterExists || !propertyExists)
                return BadRequest(new { message = "Invalid ReporterId or PropertyId" });

            var exists = await _context.Reports.AnyAsync(r =>
                r.ReporterId == dto.ReporterId && r.PropertyId == dto.PropertyId);

            if (exists)
                return Conflict(new { message = "You have already reported this property." });

            var reportId = await _idGenerator.GenerateIdAsync("reports");

            var report = new Report
            {
                ReportId = reportId,
                ReporterId = dto.ReporterId,
                PropertyId = dto.PropertyId,
                Reason = dto.Reason,
                Status = "pending",
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), new { id = report.ReportId }, new GetReportDto
            {
                ReportId = report.ReportId,
                ReporterId = report.ReporterId,
                PropertyId = report.PropertyId,
                Reason = report.Reason,
                Status = report.Status,
                Note = report.Note,
                CreatedAt = report.CreatedAt
            });
        }

        // ✅ PUT: api/Reports
        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateReportStatus([FromBody] UpdateReportStatusDto dto)
        {
            if (string.IsNullOrEmpty(dto.ReportId))
                return BadRequest(new { message = "ReportId is required." });

            var report = await _context.Reports.FindAsync(dto.ReportId);
            if (report == null)
                return NotFound(new { message = "Report not found" });

            report.Status = dto.Status;
            report.Note = dto.Note;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Report status updated" });
        }

        // ✅ DELETE: api/Reports/REP0001
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteReport(string id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound(new { message = "Report not found" });

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
