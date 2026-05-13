using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Juris.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> b)
    {
        b.ToTable("Articles");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.Category);
        b.HasIndex(x => x.PublishedAtUtc);

        b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        b.Property(x => x.Title).HasMaxLength(250).IsRequired();
        b.Property(x => x.Subtitle).HasMaxLength(400);
        b.Property(x => x.Excerpt).HasMaxLength(800);
        // Use unlimited text for both SQL Server (nvarchar(max)) and Postgres (text).
        b.Property(x => x.BodyHtml).IsRequired();
        b.Property(x => x.FeaturedImageUrl).HasMaxLength(500);
        b.Property(x => x.AuthorName).HasMaxLength(120).IsRequired();
        b.Property(x => x.AuthorUserId).HasMaxLength(450);
    }
}

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> b)
    {
        b.ToTable("NewsletterSubscribers");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.Email).HasMaxLength(254).IsRequired();
        b.Property(x => x.University).HasMaxLength(160);
        b.Property(x => x.Source).HasMaxLength(80);
        b.Property(x => x.ConfirmationToken).HasMaxLength(100);
    }
}
