using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PTJ.Domain.Entities;

namespace PTJ.Infrastructure.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("UserActivityLogs", "logging");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.HttpMethod)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(l => l.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.QueryString)
            .HasMaxLength(1000);

        builder.Property(l => l.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(l => l.UserAgent)
            .HasMaxLength(500);

        builder.Property(l => l.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.Timestamp)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.Timestamp).IsDescending();
        builder.HasIndex(l => new { l.UserId, l.Timestamp }).IsDescending();

        // Foreign key relationship
        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
