using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MainProject.API.DAL.Contexts
{
    public class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options)
    {
        public DbSet<Detail> Details { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportDetail> ReportDetails { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<User> AppUsers { get; set; }
        public DbSet<FolderUser> Users { get; set; }
        public DbSet<TableCatalog> TableCatalogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}