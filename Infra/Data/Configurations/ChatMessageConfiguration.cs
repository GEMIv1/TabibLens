using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.ChatSessionId).IsRequired();

            builder.Property(cm => cm.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            builder.Property(cm => cm.UpdatedAt).HasColumnType("timestamptz");
            builder.Property(cm => cm.IsDeleted).HasDefaultValue(false);
            builder.Property(cm => cm.DeletedAt).HasColumnType("timestamptz");

            builder.Property(cm => cm.Content).IsRequired().HasColumnType("text");
            builder.Property(cm => cm.Role).HasConversion<string>().IsRequired().HasMaxLength(50);

            builder.HasOne(cm => cm.ChatSession)
                   .WithMany(cs => cs.Messages)
                   .HasForeignKey(cm => cm.ChatSessionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cm => cm.ChatSessionId);
            builder.HasIndex(cm => cm.CreatedAt);
            builder.HasIndex(cm => cm.Role);

            builder.HasQueryFilter(cm => !cm.IsDeleted);
        }
    }
}
