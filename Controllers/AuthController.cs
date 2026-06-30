using CameraApi.Data;
using CameraApi.DTOs;
using CameraApi.Models;
using CameraApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email, Username, and Password are required");
            }

            if (request.Password.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters");
            }

            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);

            if (existingUser != null)
            {
                return Conflict("Email or Username already exists");
            }

            // Create new user
            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                PasswordHash = _tokenService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User registered: {user.Email}");

            return CreatedAtAction(nameof(Register), new RegisterResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Message = "User registered successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, "Internal server error during registration");
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and Password are required");
            }

            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning($"Login failed: User not found for email {request.Email}");
                return Unauthorized("Invalid email or password");
            }

            // Verify password
            if (!_tokenService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Login failed: Invalid password for user {request.Email}");
                return Unauthorized("Invalid email or password");
            }

            // Generate token
            var token = _tokenService.GenerateToken(user.Id, user.Email, user.Username);

            _logger.LogInformation($"User logged in: {user.Email}");

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "Internal server error during login");
        }
    }
}
