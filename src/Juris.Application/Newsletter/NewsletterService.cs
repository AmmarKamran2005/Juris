using System.Text.RegularExpressions;
using Juris.Application.Common;
using Juris.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Juris.Application.Newsletter;

public class NewsletterService : INewsletterService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly IApplicationDbContext _db;

    public NewsletterService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<NewsletterSignupResult> SignupAsync(NewsletterSignupRequest request, CancellationToken ct = default)
    {
        var email = (request.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            return new NewsletterSignupResult(false, "Please enter a valid email address.");

        var existing = await _db.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Email == email, ct);
        if (existing is not null)
        {
            existing.University = request.University ?? existing.University;
            existing.YearOfStudy = request.YearOfStudy != Domain.Enums.YearOfStudy.Unknown
                ? request.YearOfStudy : existing.YearOfStudy;
            existing.UnsubscribedAtUtc = null;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return new NewsletterSignupResult(true, "You're already on the list — preferences updated.");
        }

        _db.NewsletterSubscribers.Add(new NewsletterSubscriber
        {
            Email = email,
            University = request.University,
            YearOfStudy = request.YearOfStudy,
            Source = request.Source ?? "homepage",
            ConfirmationToken = Guid.NewGuid().ToString("N"),
            IsConfirmed = false,
        });
        await _db.SaveChangesAsync(ct);

        return new NewsletterSignupResult(true, "Welcome aboard. The 2026 Salary Report drops to your inbox next month.");
    }

    public Task<int> GetSubscriberCountAsync(CancellationToken ct = default)
        => _db.NewsletterSubscribers.CountAsync(s => s.UnsubscribedAtUtc == null, ct);
}
