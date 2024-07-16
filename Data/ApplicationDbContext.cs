using Microsoft.EntityFrameworkCore;
using MultiWeb.Models;

namespace MultiWeb.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
            new Category { Id = 2, Name = "Sci-Fi", DisplayOrder = 2 },
            new Category { Id = 3, Name = "Drama", DisplayOrder = 3 }
            );
    }

    public DbSet<Category> Categories { get; set; }   
}
