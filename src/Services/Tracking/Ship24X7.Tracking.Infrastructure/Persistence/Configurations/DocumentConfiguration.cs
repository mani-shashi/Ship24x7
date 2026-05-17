using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Tracking.Domain.Entities;

namespace Ship24X7.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Document entity. Defines table structure, relationships, and constraints.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ShipmentId)
            .IsRequired();

        builder.Property(e => e.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FileUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FileSizeBytes)
            .IsRequired();

        builder.Property(e => e.UploadedAt)
            .IsRequired();

        builder.Property(e => e.UploadedBy)
            .IsRequired();

        builder.HasIndex(e => e.ShipmentId);
        builder.HasIndex(e => e.TrackingNumber);
        builder.HasIndex(e => e.DocumentType);
    }
}
