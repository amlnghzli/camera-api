using CameraApi.Data;
using CameraApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CamerasController : ControllerBase
{
    private readonly CameraDbContext _context;
    private readonly ILogger<CamerasController> _logger;

    public CamerasController(CameraDbContext context, ILogger<CamerasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all cameras
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Camera>>> GetCameras(
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] bool activeOnly = true)
    {
        try
        {
            var query = _context.Cameras.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            var cameras = await query.ToListAsync();
            _logger.LogInformation($"Retrieved {cameras.Count} cameras");
            return Ok(cameras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cameras");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get camera by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Camera>> GetCamera(int id)
    {
        try
        {
            var camera = await _context.Cameras.FindAsync(id);

            if (camera == null)
            {
                _logger.LogWarning($"Camera with ID {id} not found");
                return NotFound();
            }

            return Ok(camera);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving camera with ID {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search cameras by brand or model
    /// </summary>
    [HttpGet("search/{term}")]
    public async Task<ActionResult<IEnumerable<Camera>>> SearchCameras(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Search term cannot be empty");
            }

            var searchTerm = term.ToLower();
            var cameras = await _context.Cameras
                .Where(c => c.IsActive && 
                    (c.Brand.ToLower().Contains(searchTerm) ||
                     c.Model.ToLower().Contains(searchTerm) ||
                     c.Type.ToLower().Contains(searchTerm)))
                .ToListAsync();

            _logger.LogInformation($"Search for '{term}' returned {cameras.Count} results");
            return Ok(cameras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching cameras with term: {term}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new camera
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Camera>> CreateCamera(Camera camera)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(camera.Brand) || string.IsNullOrWhiteSpace(camera.Model))
            {
                return BadRequest("Brand and Model are required");
            }

            camera.CreatedAt = DateTime.UtcNow;
            camera.UpdatedAt = DateTime.UtcNow;

            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Camera created: {camera.Brand} {camera.Model} with ID {camera.Id}");
            return CreatedAtAction(nameof(GetCamera), new { id = camera.Id }, camera);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating camera");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing camera
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCamera(int id, Camera camera)
    {
        try
        {
            //if (id != camera.Id)
            //
                //return BadRequest("ID mismatch");
            //}

            var existingCamera = await _context.Cameras.FindAsync(id);
            if (existingCamera == null)
            {
                return NotFound();
            }

            existingCamera.Brand = camera.Brand ?? existingCamera.Brand;
            existingCamera.Model = camera.Model ?? existingCamera.Model;
            existingCamera.Type = camera.Type ?? existingCamera.Type;
            existingCamera.Price = camera.Price > 0 ? camera.Price : existingCamera.Price;
            existingCamera.Sensor = camera.Sensor ?? existingCamera.Sensor;
            existingCamera.Megapixels = camera.Megapixels > 0 ? camera.Megapixels : existingCamera.Megapixels;
            existingCamera.Resolution = camera.Resolution ?? existingCamera.Resolution;
            existingCamera.Is4KCapable = camera.Is4KCapable;
            existingCamera.Description = camera.Description ?? existingCamera.Description;
            existingCamera.ImageUrl = camera.ImageUrl ?? existingCamera.ImageUrl;
            existingCamera.IsActive = camera.IsActive;
            existingCamera.UpdatedAt = DateTime.UtcNow;

            _context.Cameras.Update(existingCamera);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Camera updated: ID {id}");
            return Ok(existingCamera);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating camera with ID {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a camera
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCamera(int id)
    {
        try
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                return NotFound();
            }

            _context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Camera deleted: ID {id}");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting camera with ID {id}");
            return StatusCode(500, "Internal server error");
        }
    }
}
