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
    public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.UserId).IsRequired();
            builder.Property(cs => cs.PrescriptionId);

            builder.Property(cs => cs.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            builder.Property(cs => cs.UpdatedAt).HasColumnType("timestamptz");
            builder.Property(cs => cs.IsDeleted).HasDefaultValue(false);
            builder.Property(cs => cs.DeletedAt).HasColumnType("timestamptz");

            builder.Property(cs => cs.Title).IsRequired().HasMaxLength(200);

            builder.HasOne(cs => cs.User)
                   .WithMany(u => u.ChatSessions)
                   .HasForeignKey(cs => cs.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(cs => cs.Messages)
                   .WithOne(cm => cm.ChatSession)
                   .HasForeignKey(cm => cm.ChatSessionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cs => cs.Prescription)
                   .WithMany(p => p.ChatSessions)
                   .HasForeignKey(cs => cs.PrescriptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cs => cs.UserId);
            builder.HasIndex(cs => cs.PrescriptionId);
            builder.HasIndex(cs => cs.CreatedAt);

            builder.HasQueryFilter(cs => !cs.IsDeleted);
        }
    }
}
