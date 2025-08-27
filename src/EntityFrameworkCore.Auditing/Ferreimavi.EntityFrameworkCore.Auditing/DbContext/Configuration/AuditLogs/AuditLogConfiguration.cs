// ReSharper disable once CheckNamespace

namespace Mango.Auditing
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System.Text.Json;

    internal class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(e => e.Id);

            builder
                .Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(128);

            builder
                .Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(128);

            builder
                .Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(32);

            builder
                .Property(e => e.UserId)
                .HasMaxLength(128);

            builder
                .Property(e => e.UserName)
                .HasMaxLength(128);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(64);

            builder
                .Property(e => e.UserAgent)
                .HasMaxLength(512);

            builder
                .Property(e => e.OldValues)
                .HasColumnType("nvarchar(max)");

            builder
                .Property(e => e.NewValues)
                .HasColumnType("nvarchar(max)");

            builder
                .Property(e => e.AffectedColumns)
                .HasMaxLength(1024);

            builder
                .Property(e => e.PrimaryKey)
                .HasMaxLength(128);

            builder
                .Property(e => e.TableName)
                .HasMaxLength(128);

            builder
                .Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonSerializerOptions.Default) ?? new Dictionary<string, string>()
                );
        }
    }
}