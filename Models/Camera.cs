namespace CameraApi.Models;

public class Camera
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // DSLR, Mirrorless, Compact, etc.
    public decimal Price { get; set; }
    public string Sensor { get; set; } = string.Empty;
    public int Megapixels { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public bool Is4KCapable { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
