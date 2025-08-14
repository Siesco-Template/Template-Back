using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MainProject.API.DAL.Configurations
{
    public class ReportDetailConfiguration : IEntityTypeConfiguration<ReportDetail>
    {
        public void Configure(EntityTypeBuilder<ReportDetail> builder)
        {
            builder.HasOne(rd => rd.Report)
                .WithMany(r => r.ReportDetails)
                .HasForeignKey(rd => rd.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rd => rd.Detail)
                .WithMany(d => d.ReportDetails)
                .HasForeignKey(rd => rd.DetailId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}