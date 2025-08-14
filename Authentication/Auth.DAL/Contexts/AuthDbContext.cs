using Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Auth.DAL.Contexts
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<PasswordToken> PasswordTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}