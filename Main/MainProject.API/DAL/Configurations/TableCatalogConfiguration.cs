using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MainProject.API.DAL.Configurations
{
    public class TableCatalogConfiguration : IEntityTypeConfiguration<TableCatalog>
    {
        public void Configure(EntityTypeBuilder<TableCatalog> builder)
        {
            builder.HasKey(tc => tc.Id);
            builder.HasIndex(tc => new { tc.TableId, tc.CatalogPath }).IsUnique();
            builder.Property(tc => tc.TableId).IsRequired().HasMaxLength(100);
            builder.Property(tc => tc.CatalogPath).IsRequired().HasMaxLength(100);
            builder.Property(tc => tc.CatalogId).IsRequired().HasMaxLength(100);
        }
    }
}