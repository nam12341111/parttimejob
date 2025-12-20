using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PTJ.Domain.Entities;

namespace PTJ.Infrastructure.Configurations;

public class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
{
    public void Configure(EntityTypeBuilder<ChatConversation> builder)
    {
        builder.ToTable("ChatConversations", "chat");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LastMessage)
            .HasMaxLength(500);

        builder.HasIndex(c => new { c.EmployerId, c.StudentId });
        builder.HasIndex(c => c.LastMessageAt);

        builder.HasOne(c => c.Employer)
            .WithMany()
            .HasForeignKey(c => c.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Student)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.JobPost)
            .WithMany()
            .HasForeignKey(c => c.JobPostId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
