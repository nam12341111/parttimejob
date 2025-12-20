using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PTJ.Domain.Entities;

namespace PTJ.Infrastructure.Configurations;

public class SystemErrorLogConfiguration : IEntityTypeConfiguration<SystemErrorLog>
{
    public void Configure(EntityTypeBuilder<SystemErrorLog> builder)
    {
        builder.ToTable("SystemErrorLogs", "logging");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Level)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.ExceptionType)
            .HasMaxLength(500);

        builder.Property(l => l.StackTrace)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.InnerException)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.RequestPath)
            .HasMaxLength(500);

        builder.Property(l => l.HttpMethod)
            .HasMaxLength(10);

        builder.Property(l => l.QueryString)
            .HasMaxLength(1000);

        builder.Property(l => l.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(l => l.UserAgent)
            .HasMaxLength(500);

        builder.Property(l => l.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.Source)
            .HasMaxLength(200);

        builder.Property(l => l.Timestamp)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(l => l.Level);
        builder.HasIndex(l => l.Timestamp).IsDescending();
        builder.HasIndex(l => new { l.Level, l.Timestamp }).IsDescending();
        builder.HasIndex(l => l.UserId);

        // Foreign key relationship
        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
