using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Juris.Infrastructure.Persistence.Configurations;

public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> b)
    {
        b.ToTable("AnalyticsEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        // Hot indexes for HR dashboard queries.
        b.HasIndex(x => new { x.FirmId, x.TimestampUtc });
        b.HasIndex(x => new { x.SchoolId, x.TimestampUtc });
        b.HasIndex(x => new { x.ArticleId, x.TimestampUtc });
        b.HasIndex(x => x.Type);
        b.HasIndex(x => x.SessionId);
        b.HasIndex(x => x.TimestampUtc);
        b.HasIndex(x => x.UniversityAffiliation);

        b.Property(x => x.UserId).HasMaxLength(450);
        b.Property(x => x.UniversityAffiliation).HasMaxLength(160);
        b.Property(x => x.SearchTerm).HasMaxLength(400);
        b.Property(x => x.SectionKey).HasMaxLength(80);
        b.Property(x => x.Path).HasMaxLength(500).IsRequired();
        b.Property(x => x.Referrer).HasMaxLength(500);
        b.Property(x => x.UserAgent).HasMaxLength(500);
        b.Property(x => x.IpHash).HasMaxLength(80);
        // Provider-agnostic — EF picks nvarchar(max) on SQL Server, text on Postgres.
        b.Property(x => x.MetadataJson);

        b.HasOne(x => x.Firm).WithMany().HasForeignKey(x => x.FirmId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Article).WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.SetNull);
    }
}
