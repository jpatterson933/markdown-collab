using Microsoft.EntityFrameworkCore;
using MarkdownCollab.Models;

namespace MarkdownCollab.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DiagramRoom> DiagramRooms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DiagramRoom>()
            .HasIndex(r => r.RoomCode)
            .IsUnique();
    }
}
