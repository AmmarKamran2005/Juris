using Juris.Application.Analytics;
using Juris.Application.Articles;
using Juris.Application.Firms;
using Juris.Application.Import;
using Juris.Application.Newsletter;
using Juris.Application.Recruitment;
using Juris.Application.Schools;
using Juris.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Juris.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFirmService, FirmService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ICsvImportService, CsvImportService>();
        return services;
    }
}
