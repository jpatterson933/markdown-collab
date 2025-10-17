using System.ComponentModel.DataAnnotations;

namespace MarkdownCollab.Models;

public class DiagramRoom
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string RoomCode { get; set; } = string.Empty;

    [Required]
    public string DiagramContent { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
