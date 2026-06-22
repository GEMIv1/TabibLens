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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.UserId).IsRequired();

            builder.Property(rt => rt.HashedToken).IsRequired();

            builder.Property(rt => rt.CreatedAt).IsRequired().HasColumnType("timestamptz").HasDefaultValueSql("now()");
            builder.Property(rt => rt.ExpiresAt).IsRequired().HasColumnType("timestamptz");
            builder.Property(rt => rt.UpdatedAt).HasColumnType("timestamptz");
            builder.Property(rt => rt.RevokedAt).HasColumnType("timestamptz");

            builder.Property(rt => rt.IsDeleted).HasDefaultValue(false);
            builder.Property(rt => rt.DeletedAt).HasColumnType("timestamptz");

            builder.Ignore(rt => rt.IsExpired);
            builder.Ignore(rt => rt.IsRevoked);
            builder.Ignore(rt => rt.IsActive);

            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rt => rt.HashedToken).IsUnique();
            builder.HasIndex(rt => rt.ExpiresAt);

            builder.HasQueryFilter(rt => !rt.IsDeleted);
        }
    }
}
