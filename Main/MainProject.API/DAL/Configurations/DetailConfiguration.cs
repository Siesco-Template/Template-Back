using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MainProject.API.DAL.Configurations
{
    public class DetailConfiguration : IEntityTypeConfiguration<Detail>
    {
        public void Configure(EntityTypeBuilder<Detail> builder)
        {

            builder.HasOne(d => d.ParentDetail)
            .WithMany(d => d.Childrens)
            .HasForeignKey(d => d.ParentDetailId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}