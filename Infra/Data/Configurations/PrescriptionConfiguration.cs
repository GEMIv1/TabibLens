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
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.UserId).IsRequired();

            builder.Property(p => p.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            builder.Property(p => p.UpdatedAt).HasColumnType("timestamptz");
            builder.Property(p => p.IsDeleted).HasDefaultValue(false);
            builder.Property(p => p.DeletedAt).HasColumnType("timestamptz");

            builder.Property(p => p.OcrRawData).HasColumnType("jsonb");
            builder.Property(p => p.FailureReason).HasColumnType("text");

            builder.Property(p => p.OcrProcessedAt).HasColumnType("timestamptz");

            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(50);

            builder.HasOne(p => p.User)
                   .WithMany(u => u.Prescriptions)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Medications)
                   .WithOne(m => m.Prescription)
                   .HasForeignKey(m => m.PrescriptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.ChatSessions)
                   .WithOne(cs => cs.Prescription)
                   .HasForeignKey(cs => cs.PrescriptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => p.Status);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
