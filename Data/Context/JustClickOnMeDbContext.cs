using Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class JustClickOnMeDbContext : IdentityDbContext<User>
{
    public JustClickOnMeDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Link> Links { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var link = builder.Entity<Link>();
        link.HasKey(l => l.Slug);

        var user = builder.Entity<User>();
        user.HasMany(u => u.Links).WithOne().HasForeignKey(l => l.UserId);
    }
}