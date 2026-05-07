using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Juris.Infrastructure.Persistence.Configurations;

public class FirmPracticeAreaConfiguration : IEntityTypeConfiguration<FirmPracticeArea>
{
    public void Configure(EntityTypeBuilder<FirmPracticeArea> b)
    {
        b.ToTable("FirmPracticeAreas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(120).IsRequired();
        b.HasIndex(x => new { x.FirmId, x.Slug }).IsUnique();
        b.HasIndex(x => x.Slug);
    }
}

public class FirmPerkConfiguration : IEntityTypeConfiguration<FirmPerk>
{
    public void Configure(EntityTypeBuilder<FirmPerk> b)
    {
        b.ToTable("FirmPerks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Category).HasMaxLength(80).IsRequired();
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
    }
}

public class FirmReviewConfiguration : IEntityTypeConfiguration<FirmReview>
{
    public void Configure(EntityTypeBuilder<FirmReview> b)
    {
        b.ToTable("FirmReviews");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Body).HasMaxLength(8000).IsRequired();
        b.Property(x => x.AuthorAlias).HasMaxLength(80);
    }
}

public class FirmDeadlineConfiguration : IEntityTypeConfiguration<FirmDeadline>
{
    public void Configure(EntityTypeBuilder<FirmDeadline> b)
    {
        b.ToTable("FirmDeadlines");
        b.HasKey(x => x.Id);
        b.Property(x => x.Label).HasMaxLength(160).IsRequired();
        b.Property(x => x.ApplyUrl).HasMaxLength(500);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.HasIndex(x => x.DeadlineUtc);
    }
}
