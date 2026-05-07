using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Juris.Infrastructure.Persistence.Configurations;

public class FirmConfiguration : IEntityTypeConfiguration<Firm>
{
    public void Configure(EntityTypeBuilder<Firm> builder)
    {
        builder.ToTable("Firms");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.IsVerified);
        builder.HasIndex(x => x.PeerGroup);

        builder.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LegalName).HasMaxLength(250);
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Province).HasMaxLength(50);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(500);
        builder.Property(x => x.ApplyUrl).HasMaxLength(500);
        builder.Property(x => x.EarlyCareersUrl).HasMaxLength(500);
        builder.Property(x => x.RetentionRate).HasPrecision(5, 2);
        builder.Property(x => x.AiPolicy).HasMaxLength(4000);
        builder.Property(x => x.WorkLifeBalance).HasMaxLength(4000);
        builder.Property(x => x.OfficeCulture).HasMaxLength(4000);
        builder.Property(x => x.CompensationPhilosophy).HasMaxLength(4000);
        builder.Property(x => x.VibeSummary).HasMaxLength(2000);
        builder.Property(x => x.OwnerUserId).HasMaxLength(450);

        builder.HasMany(x => x.PracticeAreas)
            .WithOne(x => x.Firm!)
            .HasForeignKey(x => x.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Perks)
            .WithOne(x => x.Firm!)
            .HasForeignKey(x => x.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reviews)
            .WithOne(x => x.Firm!)
            .HasForeignKey(x => x.FirmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Deadlines)
            .WithOne(x => x.Firm!)
            .HasForeignKey(x => x.FirmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
