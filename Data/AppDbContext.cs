using Microsoft.EntityFrameworkCore;
using Pgvector;
using ESSChatbot.Models;

namespace ESSChatbot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<HrPolicyChunk> HrPolicyChunks { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension("vector");
    modelBuilder.Entity<HrPolicyChunk>()
        .ToTable("hr_policy_chunks")
        .Property(e => e.Embedding)
        .HasColumnType("vector(1536)");
}
}