using Backend.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contractor> Contractors => Set<Contractor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contractor>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.CompanyName);

            entity.Property(x => x.ContractorNumber)
            .HasMaxLength(50)
            .IsRequired();

            entity.HasIndex(x => x.ContractorNumber)
                .IsUnique();

            entity.Property(x => x.RowVersion)
                    .IsRowVersion();
        });
    }
}