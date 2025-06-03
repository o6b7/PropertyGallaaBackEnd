using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropertyGalla.Data;
using PropertyGalla.DTOs.UserDTOs;
using PropertyGalla.Models;
using PropertyGalla.Services;

namespace PropertyGalla.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly PropertyGallaContext _context;
        private readonly IdGeneratorService _idGenerator;
        private readonly IConfiguration _config;

        public UsersController(PropertyGallaContext context, IConfiguration config)
        {
            _context = context;
            _idGenerator = new IdGeneratorService(context);
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !PasswordService.VerifyPassword(loginDto.Password, user.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new UserResponseDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!PasswordService.VerifyPassword(changePasswordDto.OldPassword, user.Password))
                return Unauthorized(new { message = "Incorrect old password" });

            user.Password = PasswordService.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return BadRequest(new { message = "Email is already taken" });

            var user = new User
            {
                UserId = await _idGenerator.GenerateIdAsync("users"),
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = PasswordService.HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", user.UserId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers([FromQuery] string? name = null, [FromQuery] string? email = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.Name.Contains(name));
            if (!string.IsNullOrEmpty(email))
                query = query.Where(u => u.Email.Contains(email));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new { currentPage = page, totalPages, totalCount, pageSize, users });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto userDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != id)
                return Forbid("You can only modify your own account.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!PasswordService.VerifyPassword(userDto.Password, user.Password))
                return Unauthorized(new { message = "Incorrect password" });

            user.Name = userDto.Name;
            user.Phone = userDto.Phone;
            user.Email = userDto.Email;
            user.Role = userDto.Role;

            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully" });
        }
        

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // 1. Remove feedbacks (given or received)
            _context.Feedbacks.RemoveRange(
                _context.Feedbacks.Where(f => f.ReviewerId == id || f.OwnerId == id)
            );

            // 2. Remove reports submitted by this user
            _context.Reports.RemoveRange(
                _context.Reports.Where(r => r.ReporterId == id)
            );

            // 3. Remove view requests made by this user
            _context.ViewRequests.RemoveRange(
                _context.ViewRequests.Where(v => v.UserId == id)
            );

            // 4. Remove saved properties saved by this user
            _context.SavedProperties.RemoveRange(
                _context.SavedProperties.Where(sp => sp.UserId == id)
            );

            // 5. Remove all properties owned by the user and their dependencies
            var properties = await _context.Properties
                .Include(p => p.Images)
                .Where(p => p.OwnerId == id)
                .ToListAsync();

            foreach (var property in properties)
            {
                var propId = property.PropertyId;

                _context.PropertyImages.RemoveRange(property.Images);
                _context.ViewRequests.RemoveRange(_context.ViewRequests.Where(v => v.PropertyId == propId));
                _context.SavedProperties.RemoveRange(_context.SavedProperties.Where(sp => sp.PropertyId == propId));
                _context.Reports.RemoveRange(_context.Reports.Where(r => r.PropertyId == propId));
            }

            _context.Properties.RemoveRange(properties);

            // 6. Finally delete the user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
