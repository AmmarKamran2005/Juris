using Juris.Application.Common;
using Juris.Domain.Entities;
using Juris.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Juris.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Firm> Firms => Set<Firm>();
    public DbSet<FirmPracticeArea> FirmPracticeAreas => Set<FirmPracticeArea>();
    public DbSet<FirmPerk> FirmPerks => Set<FirmPerk>();
    public DbSet<FirmReview> FirmReviews => Set<FirmReview>();
    public DbSet<FirmDeadline> FirmDeadlines => Set<FirmDeadline>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
