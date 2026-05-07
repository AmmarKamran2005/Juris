using Juris.Domain.Enums;

namespace Juris.Application.Newsletter;

public sealed record NewsletterSignupRequest(
    string Email,
    string? University,
    YearOfStudy YearOfStudy,
    string Source = "homepage"
);

public sealed record NewsletterSignupResult(bool Success, string Message);
