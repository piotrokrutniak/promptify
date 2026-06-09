using PromptifyWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PromptifyWebApi.Infrastructure.Data.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.Property(p => p.Input)
            .HasMaxLength(8000)
            .IsRequired();

        builder.Property(p => p.Output)
            .HasMaxLength(16000);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(p => p.Data)
            .HasColumnType("jsonb");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(p => new { p.SessionId, p.OrderIndex })
            .IsUnique();

        builder.HasIndex(p => new { p.Status, p.SessionId });
    }
}
