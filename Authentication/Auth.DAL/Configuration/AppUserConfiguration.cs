using Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.DAL.Configuration
{
    class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.FirstName).HasMaxLength(32);

            builder.Property(x => x.LastName).HasMaxLength(32);

            builder.Property(x => x.Email).HasMaxLength(128);

            builder.Property(x => x.PhoneNumber).HasMaxLength(128);

            builder.Property(x => x.Password).HasMaxLength(64);

            builder.HasMany(x => x.LoginLogs)
                .WithOne(x => x.AppUser)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PasswordToken)
                .WithOne(x => x.AppUser)
                .HasForeignKey<PasswordToken>(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}