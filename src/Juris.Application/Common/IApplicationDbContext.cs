using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Common;

/// <summary>
/// Application-side abstraction over the EF Core context. Implemented by
/// Infrastructure. Lets services in Application query and persist without
/// taking a hard reference on the concrete DbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Firm> Firms { get; }
    DbSet<FirmPracticeArea> FirmPracticeAreas { get; }
    DbSet<FirmPerk> FirmPerks { get; }
    DbSet<FirmReview> FirmReviews { get; }
    DbSet<FirmDeadline> FirmDeadlines { get; }
    DbSet<School> Schools { get; }
    DbSet<Article> Articles { get; }
    DbSet<NewsletterSubscriber> NewsletterSubscribers { get; }
    DbSet<AnalyticsEvent> AnalyticsEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
