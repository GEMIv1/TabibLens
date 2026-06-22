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
    public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
    {
        public void Configure(EntityTypeBuilder<Medication> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.PrescriptionId).IsRequired();

            builder.Property(m => m.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            builder.Property(m => m.UpdatedAt).HasColumnType("timestamptz");
            builder.Property(m => m.IsDeleted).HasDefaultValue(false);
            builder.Property(m => m.DeletedAt).HasColumnType("timestamptz");

            builder.Property(m => m.DrugRawData).IsRequired();
            builder.Property(m => m.DrugNameNormalized).HasMaxLength(256);
            builder.Property(m => m.DoseRaw).HasMaxLength(100);
            builder.Property(m => m.FrequencyRaw).HasMaxLength(100);
            builder.Property(m => m.DurationRaw).HasMaxLength(50);
            builder.Property(m => m.StrengthRaw).HasMaxLength(50);
            builder.Property(m => m.ConfidenceScore).IsRequired();
            builder.Property(m => m.DosageForm).HasConversion<string>().HasMaxLength(50);

            builder.HasOne(m => m.Prescription)
                   .WithMany(p => p.Medications)
                   .HasForeignKey(m => m.PrescriptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => m.PrescriptionId);
            builder.HasIndex(m => m.DrugNameNormalized);
            builder.HasIndex(m => m.DosageForm);


            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}
