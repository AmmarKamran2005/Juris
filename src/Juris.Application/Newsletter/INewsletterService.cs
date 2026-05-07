namespace Juris.Application.Newsletter;

public interface INewsletterService
{
    Task<NewsletterSignupResult> SignupAsync(NewsletterSignupRequest request, CancellationToken ct = default);
    Task<int> GetSubscriberCountAsync(CancellationToken ct = default);
}
