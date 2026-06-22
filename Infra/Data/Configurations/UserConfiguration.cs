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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.UserName).IsRequired().HasMaxLength(100).HasColumnType("citext");
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256).HasColumnType("citext");
            builder.Property(u => u.PhoneNumber).HasMaxLength(16);
            builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(u => u.HashedPassword).IsRequired();
            builder.Property(u => u.CreatedAt).HasColumnType("timestamptz").IsRequired().HasDefaultValueSql("now()");
            builder.Property(u => u.UpdatedAt).HasColumnType("timestamptz");
            builder.Property(u => u.DeletedAt).HasColumnType("timestamptz");
            builder.Property(u => u.EmailConfirmedAt).HasColumnType("timestamptz");
            builder.Property(u => u.LastLoginAt).HasColumnType("timestamptz");
            builder.Property(u => u.IsDeleted).HasDefaultValue(false);

            builder.HasIndex(u => u.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
            builder.HasIndex(u => u.UserName).IsUnique().HasFilter("\"IsDeleted\" = false");

            builder.HasMany(u => u.RefreshTokens)
                   .WithOne(rt => rt.User)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Prescriptions)
                   .WithOne(p => p.User)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.ChatSessions)
                   .WithOne(cs => cs.User)
                   .HasForeignKey(cs => cs.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
