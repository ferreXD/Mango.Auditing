// ReSharper disable once CheckNamespace

namespace EntityFrameworkCore.Auditing.Tests.UnitTests.Setup
{
    using Mango.Auditing;
    using Microsoft.EntityFrameworkCore;

    public class TestDbContext(
        DbContextOptions contextOptions, 
        AuditingOptions auditingOptions) 
        : AuditableDbContext(contextOptions, auditingOptions)
    {
        public DbSet<SampleEntity> SampleEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base implementation for common model configurations (auditing, soft deletion filters, etc.)
            base.OnModelCreating(modelBuilder);

            // Configure SampleEntity. Adjust configuration as needed.
            modelBuilder.Entity<SampleEntity>(entity =>
            {
                // Use Id as the primary key.
                entity.HasKey(e => e.Id);

                // Name is required.
                entity.Property(e => e.Name).IsRequired();

                // Description is optional with a maximum length.
                entity.Property(e => e.Description).HasMaxLength(1024);
            });
        }
    }
}
