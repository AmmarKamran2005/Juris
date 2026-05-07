using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Juris.Infrastructure.Persistence.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> b)
    {
        b.ToTable("Schools");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.City);

        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ShortName).HasMaxLength(80);
        b.Property(x => x.City).HasMaxLength(100).IsRequired();
        b.Property(x => x.Province).HasMaxLength(50);
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.Property(x => x.WebsiteUrl).HasMaxLength(500);

        b.Property(x => x.MedianLsat).HasPrecision(5, 2);
        b.Property(x => x.MedianGpa).HasPrecision(4, 2);
        b.Property(x => x.EmploymentRateAt9MonthsPct).HasPrecision(5, 2);
        b.Property(x => x.SevenSistersHireRatePct).HasPrecision(5, 2);
        b.Property(x => x.BarPassRatePct).HasPrecision(5, 2);

        b.Property(x => x.AboutHtml).HasMaxLength(8000);
        b.Property(x => x.MootingNotes).HasMaxLength(4000);
        b.Property(x => x.ClinicNotes).HasMaxLength(4000);
        b.Property(x => x.PreLawPathwayNotes).HasMaxLength(4000);
        b.Property(x => x.CareerOutcomesNotes).HasMaxLength(4000);
    }
}
