using Juris.Domain.Entities;
using Juris.Domain.Enums;
using Juris.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Juris.Web.Infrastructure;

/// <summary>
/// Idempotent placeholder data. Real client CSV import will replace this
/// later — but until then the platform looks fully populated.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger)
    {
        if (!await db.Firms.AnyAsync())
        {
            db.Firms.AddRange(BuildFirms());
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} firms", await db.Firms.CountAsync());
        }

        if (!await db.Schools.AnyAsync())
        {
            db.Schools.AddRange(BuildSchools());
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} schools", await db.Schools.CountAsync());
        }

        if (!await db.Articles.AnyAsync())
        {
            db.Articles.AddRange(BuildArticles());
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} articles", await db.Articles.CountAsync());
        }
    }

    private static List<Firm> BuildFirms()
    {
        var now = DateTime.UtcNow;
        DateTime Oci(int daysAhead) => DateTime.UtcNow.Date.AddDays(daysAhead).AddHours(21); // 5pm ET-ish
        DateTime Articling(int daysAhead) => DateTime.UtcNow.Date.AddDays(daysAhead).AddHours(21);

        Firm Firm(
            string slug, string name, string city, PeerGroup group,
            int salary, int classSize, decimal retention, int billable,
            string vibe, string ai, string wlb, string culture,
            int ociIn, int articlingIn,
            string[] practiceAreas, (string cat, string nm, string desc)[] perks,
            bool verified = false, bool featured = false)
        {
            var f = new Firm
            {
                Slug = slug,
                Name = name,
                City = city,
                Province = "Ontario",
                LogoUrl = $"/img/firms/{slug}.png",
                WebsiteUrl = $"https://www.{slug}.example",
                ApplyUrl = $"https://careers.{slug}.example/apply",
                EarlyCareersUrl = $"https://careers.{slug}.example/students",
                ArticlingSalary = salary,
                FirstYearAssociateSalary = salary + 30000,
                ClassSize = classSize,
                RetentionRate = retention,
                BillableTarget = billable,
                OciDeadlineUtc = Oci(ociIn),
                ArticlingDeadlineUtc = Articling(articlingIn),
                VibeSummary = vibe,
                AiPolicy = ai,
                WorkLifeBalance = wlb,
                OfficeCulture = culture,
                CompensationPhilosophy = "Lockstep first-year compensation, market-matching at lateral entry. Reviews are calibrated against the broader Bay Street cohort.",
                PeerGroup = group,
                IsVerified = verified,
                IsFeaturedPartner = featured,
                IsPublished = true,
            };

            for (int i = 0; i < practiceAreas.Length; i++)
            {
                f.PracticeAreas.Add(new FirmPracticeArea
                {
                    Name = practiceAreas[i],
                    Slug = practiceAreas[i].ToLowerInvariant().Replace(" & ", "-").Replace(' ', '-'),
                    IsCorePractice = i < 3,
                });
            }

            for (int i = 0; i < perks.Length; i++)
            {
                f.Perks.Add(new FirmPerk
                {
                    Category = perks[i].cat,
                    Name = perks[i].nm,
                    Description = perks[i].desc,
                    DisplayOrder = i,
                });
            }

            f.Deadlines.Add(new FirmDeadline
            {
                Type = DeadlineType.Oci,
                Label = "Toronto OCI Application",
                DeadlineUtc = Oci(ociIn),
                ApplyUrl = f.ApplyUrl,
                Notes = "Apply via viLaw / firm portal.",
            });
            f.Deadlines.Add(new FirmDeadline
            {
                Type = DeadlineType.Articling,
                Label = "Articling Recruit",
                DeadlineUtc = Articling(articlingIn),
                ApplyUrl = f.ApplyUrl,
            });

            f.Reviews.Add(new FirmReview
            {
                Title = $"Inside the {name} articling year",
                Body = $"Editorial profile of {name}. Class culture skews collaborative; mentorship is partner-led with formal feedback at the 3-, 6-, and 9-month marks. Hours peak in Q1 around closings; junior associates report regular 8-9pm finishes outside of deal cycles.",
                AuthorAlias = "Juris Editorial",
                IsAnonymous = false,
                IsEditorial = true,
                PublishedAtUtc = now.AddDays(-21),
            });
            f.Reviews.Add(new FirmReview
            {
                Title = "What summer students actually said",
                Body = "Anonymous summer cohort feedback: the social calendar is meaningful (not perfunctory), the work is real (not make-work), and feedback is direct. The biggest surprise was the AI rollout — students were asked to test review workflows in week two.",
                AuthorAlias = $"2L Summer at {name}",
                IsAnonymous = true,
                IsEditorial = false,
                PublishedAtUtc = now.AddDays(-7),
            });

            return f;
        }

        return new List<Firm>
        {
            Firm("blake-cassels-graydon", "Blake, Cassels & Graydon LLP", "Toronto", PeerGroup.SevenSisters,
                115000, 28, 78m, 1750,
                "A pillar of the Seven Sisters with deep capital markets and M&A practices. Sharp, hierarchical, but invests heavily in junior development.",
                "Generative AI is permitted internally with sandboxed tooling; client-facing output requires partner review. Mandatory training in week one.",
                "Billable target is 1,750. Real hours trend higher on transactional teams during deal cycles. Wellness allowance of $750/year.",
                "Black-tie firm dinners, structured mentorship, and a long history of producing partners from within. Office culture is formal but warm at the associate level.",
                14, 60,
                new[] { "Mergers & Acquisitions", "Capital Markets", "Litigation", "Tax", "Banking" },
                new[]
                {
                    ("AI Training", "Internal AI sandbox", "Two-day onboarding on the firm's vetted generative AI stack."),
                    ("Wellness", "$750 wellness allowance", "Annual wellness reimbursement covering gyms, therapy, and equipment."),
                    ("Hybrid Work", "3 days in office", "Hybrid policy with 3 in-office days; flexibility increases at the senior associate level."),
                    ("Parental Leave", "Top-up to 95% for 17 weeks", "Industry-leading parental top-up with phased return programme."),
                },
                verified: true, featured: true),

            Firm("osler-hoskin-harcourt", "Osler, Hoskin & Harcourt LLP", "Toronto", PeerGroup.SevenSisters,
                115000, 26, 81m, 1700,
                "Deeply national platform with anchor practices in M&A, tax, and energy. Known for the 'Osler way': structured training, exhaustive precedents.",
                "AI tools are allowed with strict matter-flagging. Client confidentiality reviews are mandatory before any new tool is approved.",
                "1,700-hour target. Vacation expectation is genuinely 4 weeks. Friday early-finish in summer.",
                "Cohort identity is strong — the articling class travels together, eats together, and is reviewed together. Less hierarchical than peers in day-to-day work.",
                10, 55,
                new[] { "Tax", "M&A", "Energy", "Litigation", "Pensions & Benefits" },
                new[]
                {
                    ("AI Training", "Vetted toolchain", "Firm-approved AI stack with mandatory matter-flagging workflow."),
                    ("Hybrid Work", "Flex days", "2 mandatory in-office days, the rest are flex."),
                    ("Wellness", "$1,000 lifestyle spending", "Annual lifestyle allowance covering wellness, learning, and home office."),
                },
                verified: true),

            Firm("torys", "Torys LLP", "Toronto", PeerGroup.SevenSisters,
                115000, 22, 80m, 1750,
                "Smaller class size for a Seven Sister, which means more responsibility on transactions earlier. Strong cross-border practice.",
                "AI policy is conservative externally and progressive internally — junior associates run AI-assisted due diligence on most matters.",
                "1,750 hours. Real life: high during deal cycles, breathable in between. Partners are visible.",
                "The smallest of the Seven Sisters by headcount and it shows in the culture — partners know your name and what files you are on.",
                12, 58,
                new[] { "M&A", "Capital Markets", "Banking", "Litigation" },
                new[]
                {
                    ("Mentorship", "Partner mentor at intake", "Each articling student is paired with a partner mentor on day one."),
                    ("Wellness", "$1,200 wellness allowance", "Includes therapy, fitness, and ergonomic equipment."),
                    ("Hybrid Work", "Flex 2-3 days", "2-3 mandatory in-office days depending on practice group."),
                },
                verified: false),

            Firm("davies-ward-phillips-vineberg", "Davies Ward Phillips & Vineberg LLP", "Toronto", PeerGroup.SevenSisters,
                120000, 18, 85m, 1900,
                "Smallest by headcount, largest by revenue per lawyer. The deal-machine — high billable expectations, top compensation.",
                "Internal AI use is encouraged, but external AI tools are blocked. Davies has built proprietary research tooling.",
                "1,900 hours. Davies is candid about expectations — this is a high-output environment with above-market pay to match.",
                "Direct, transactional, no-nonsense. The class is small and the work is dense from day one.",
                9, 50,
                new[] { "M&A", "Tax", "Capital Markets", "Litigation" },
                new[]
                {
                    ("Compensation", "Top of market", "Davies pays a meaningful premium across all years."),
                    ("AI Training", "Proprietary tooling", "Internal research stack with junior associate training."),
                    ("Wellness", "$1,500 wellness allowance", "Among the highest on the Street."),
                },
                verified: true, featured: true),

            Firm("mccarthy-tetrault", "McCarthy Tétrault LLP", "Toronto", PeerGroup.SevenSisters,
                115000, 30, 76m, 1700,
                "Largest articling class in the country — a true cohort experience. National platform, deep bench in every practice.",
                "AI Centre of Excellence drives policy. Generative AI is permitted across most workflows with audit trails.",
                "1,700 hours target with a real culture of taking vacation. The size of the firm cushions deal-cycle peaks.",
                "Network-rich firm. The class will scatter across offices but stays connected via firm-wide secondments.",
                15, 60,
                new[] { "Litigation", "M&A", "Banking", "Capital Markets", "Real Estate" },
                new[]
                {
                    ("AI Training", "AI Centre of Excellence", "Dedicated team supports juniors using AI in research and drafting."),
                    ("Mobility", "Cross-office secondments", "Toronto students can secondment to Vancouver, Calgary, Montreal."),
                    ("Wellness", "$1,000 wellness allowance", ""),
                },
                verified: true),

            Firm("goodmans", "Goodmans LLP", "Toronto", PeerGroup.NationalFullService,
                115000, 16, 82m, 1750,
                "Boutique-feel inside a full-service firm. Renowned restructuring practice, top-tier capital markets.",
                "AI use is partner-supervised. Client matters require partner sign-off before any external tool is engaged.",
                "1,750 hours. Lean class means heavy responsibility, but partners are accessible.",
                "Goodmans punches above its weight. Smaller class, higher signal, more partner interaction than the Seven Sisters average.",
                11, 56,
                new[] { "Restructuring", "Capital Markets", "M&A", "Litigation" },
                new[]
                {
                    ("Mentorship", "1:2 partner ratio", "Partner-to-student ratio is the lowest in the city."),
                    ("Wellness", "$900 lifestyle spending", ""),
                },
                verified: false),

            Firm("stikeman-elliott", "Stikeman Elliott LLP", "Toronto", PeerGroup.SevenSisters,
                115000, 32, 75m, 1750,
                "Heritage M&A and securities house. International network through the Stikeman global desks.",
                "Pragmatic AI policy: approved tools for research and drafting; client confidentiality is non-negotiable.",
                "1,750 hours. Cohort identity is strong; the class often works late together rather than in isolation.",
                "Tradition-forward, business-formal. Strong international rotation programme for senior associates.",
                13, 59,
                new[] { "M&A", "Securities", "Banking", "Tax", "Competition" },
                new[]
                {
                    ("Mobility", "International rotations", "Senior associate secondments to NYC, London, Sydney."),
                    ("Hybrid Work", "3 days in", "3 mandatory in-office days."),
                },
                verified: true),

            Firm("fasken", "Fasken Martineau DuMoulin LLP", "Toronto", PeerGroup.NationalFullService,
                110000, 24, 79m, 1700,
                "True national footprint with strong mining, energy, and labour & employment practices. Less Bay-Street-centric than the Seven Sisters.",
                "AI tools are permitted with mandatory training and matter-level disclosure to clients.",
                "1,700 hours. Reasonable expectations relative to compensation. Friday early-finish in summer is real.",
                "Collegial, accessible, slightly less formal. Open-door policy is genuine at the partner level.",
                14, 62,
                new[] { "Labour & Employment", "Mining", "Energy", "Litigation", "Corporate" },
                new[]
                {
                    ("Mentorship", "Two-mentor model", "Each student is paired with both a partner and a senior associate."),
                    ("Wellness", "$800 lifestyle allowance", ""),
                    ("Hybrid Work", "2 days in office", "Mandatory 2 days, with practice-group flexibility."),
                },
                verified: false),

            Firm("lerners", "Lerners LLP", "Hamilton", PeerGroup.MidLaw,
                95000, 8, 88m, 1600,
                "A respected regional litigation house with a cult-favourite reputation. Class size is small; responsibility comes early.",
                "Cautious AI policy reflecting an insurance-defence and litigation client base — explicit consent for AI-assisted work.",
                "1,600 hours target. Strong commitment to hours-fairness and weekend protection.",
                "Tight-knit, regionally proud, deeply mentorship-oriented. Hamilton office has its own identity within the firm.",
                21, 75,
                new[] { "Litigation", "Class Actions", "Insurance Defence", "Personal Injury" },
                new[]
                {
                    ("Mentorship", "Direct partner work", "Articling students draft alongside partners on litigation matters."),
                    ("Wellness", "Genuine 4-week vacation", "Vacation is taken, not banked."),
                },
                verified: false),

            Firm("gowling-wlg", "Gowling WLG", "Ottawa", PeerGroup.NationalFullService,
                108000, 20, 80m, 1700,
                "Federal and IP powerhouse with a deeply Ottawa identity. Strongest tech-transfer and patent practices in Canada.",
                "Open AI policy with internal tools for prior-art search and contract drafting.",
                "1,700 hours. Public-sector adjacency moderates extreme hours. Strong on flexibility post-articling.",
                "Tech and government in equal measure. Bilingual capability is genuinely valued and rewarded.",
                16, 64,
                new[] { "Intellectual Property", "Government", "Technology", "Litigation" },
                new[]
                {
                    ("Languages", "Bilingual premium", "Genuine French/English work for those with capacity."),
                    ("AI Training", "Patent-focused tooling", "Specialist AI tools for prior-art search and patent drafting."),
                },
                verified: false),

            Firm("kpmg-law", "KPMG Law LLP", "Toronto", PeerGroup.MidLaw,
                100000, 10, 84m, 1500,
                "Lower hours, lower pay, but unique commercial exposure. Embedded with a Big Four advisory practice.",
                "AI use is governed by KPMG's global responsible-AI framework — among the most documented in the legal market.",
                "1,500 hours. Strict 8-6 culture in most practice groups outside of audit-driven cycles.",
                "Less white-shoe, more advisory-firm. Good fit for students with business-school backgrounds or commercial appetite.",
                18, 70,
                new[] { "Tax", "Corporate", "Immigration" },
                new[]
                {
                    ("Hybrid Work", "Genuine remote-first", "Two days in office per week, no rigid practice-group rules."),
                    ("Compensation", "Lower base, higher upside", "Base is below Bay Street, but partner track is faster."),
                },
                verified: false),

            Firm("paliare-roland", "Paliare Roland Rosenberg Rothstein LLP", "Toronto", PeerGroup.Boutique,
                100000, 6, 90m, 1600,
                "Premier appellate and complex commercial litigation boutique. Small classes, partner-led work from day one.",
                "Conservative AI policy due to confidentiality concerns in litigation — internal tools only, no external services.",
                "1,600 hours. The work is intellectually demanding rather than volume-heavy.",
                "Genuinely intellectual culture. The firm reads, writes, and argues — and expects the same of its students.",
                21, 90,
                new[] { "Appellate", "Class Actions", "Complex Commercial Litigation", "Constitutional" },
                new[]
                {
                    ("Mentorship", "Co-counsel from year one", "Articling students sit second-chair on appellate matters."),
                    ("Compensation", "Above-boutique average", ""),
                },
                verified: false),
        };
    }

    private static List<School> BuildSchools()
    {
        return new List<School>
        {
            new()
            {
                Slug = "uoft-law", Name = "University of Toronto Faculty of Law", ShortName = "UofT", City = "Toronto",
                LogoUrl = "/img/schools/uoft.png", WebsiteUrl = "https://www.law.utoronto.ca",
                AnnualTuitionCad = 38000, ClassSize = 220, MedianLsat = 167, MedianGpa = 3.85m,
                EmploymentRateAt9MonthsPct = 96, SevenSistersHireRatePct = 38, BarPassRatePct = 95,
                AboutHtml = "<p>The most selective law school in Canada and the most prolific feeder of the Bay Street Seven Sisters.</p>",
                MootingNotes = "Fox, Davies, and Laskin moots are well-resourced and produce annual finalists.",
                ClinicNotes = "Downtown Legal Services and PBSC chapters offer year-round clinical placements.",
                CareerOutcomesNotes = "Highest median starting salary in the country; strongest cross-border outcomes.",
            },
            new()
            {
                Slug = "osgoode", Name = "Osgoode Hall Law School (York University)", ShortName = "Osgoode", City = "Toronto",
                LogoUrl = "/img/schools/osgoode.png", WebsiteUrl = "https://www.osgoode.yorku.ca",
                AnnualTuitionCad = 30000, ClassSize = 290, MedianLsat = 162, MedianGpa = 3.75m,
                EmploymentRateAt9MonthsPct = 93, SevenSistersHireRatePct = 28, BarPassRatePct = 94,
                AboutHtml = "<p>Largest common-law school in the country with the broadest practice-area exposure.</p>",
                MootingNotes = "Renowned mooting programme — Osgoode regularly fields more national finalists than any other Canadian school.",
                ClinicNotes = "Parkdale Community Legal Services is the flagship clinic — full-time placement counts as a semester.",
                PreLawPathwayNotes = "Strong intake from Ontario undergrad programmes, particularly social science and humanities backgrounds.",
            },
            new()
            {
                Slug = "queens-law", Name = "Queen's University Faculty of Law", ShortName = "Queen's", City = "Kingston",
                LogoUrl = "/img/schools/queens.png", WebsiteUrl = "https://law.queensu.ca",
                AnnualTuitionCad = 27500, ClassSize = 200, MedianLsat = 161, MedianGpa = 3.78m,
                EmploymentRateAt9MonthsPct = 92, SevenSistersHireRatePct = 24, BarPassRatePct = 93,
                AboutHtml = "<p>Smaller cohort, strong alumni network, deep ties to Bay Street and federal government.</p>",
                MootingNotes = "Strong international moots — Jessup and Concours Charles-Rousseau both well-supported.",
                ClinicNotes = "Queen's Legal Aid is one of the oldest student-run clinics in the country.",
            },
            new()
            {
                Slug = "western-law", Name = "Western University Faculty of Law", ShortName = "Western", City = "London",
                LogoUrl = "/img/schools/western.png", WebsiteUrl = "https://law.uwo.ca",
                AnnualTuitionCad = 28500, ClassSize = 180, MedianLsat = 161, MedianGpa = 3.72m,
                EmploymentRateAt9MonthsPct = 91, SevenSistersHireRatePct = 22, BarPassRatePct = 92,
                AboutHtml = "<p>Business-law-forward curriculum with deep Ivey-adjacent corporate finance exposure.</p>",
                CareerOutcomesNotes = "Strongest pipeline into corporate, M&A, and capital markets practices.",
            },
            new()
            {
                Slug = "ottawa-law-common-law", Name = "University of Ottawa — Common Law", ShortName = "uOttawa", City = "Ottawa",
                LogoUrl = "/img/schools/uottawa.png", WebsiteUrl = "https://commonlaw.uottawa.ca",
                AnnualTuitionCad = 22000, ClassSize = 320, MedianLsat = 158, MedianGpa = 3.68m,
                EmploymentRateAt9MonthsPct = 89, SevenSistersHireRatePct = 14, BarPassRatePct = 91,
                AboutHtml = "<p>Largest common-law school by intake, with anchor strength in public, IP, and technology law.</p>",
                CareerOutcomesNotes = "Strongest pipeline into federal government, regulators, and Ottawa-based IP boutiques.",
                PreLawPathwayNotes = "Bilingual JD pathway; National Programme allows direct civil-law transfer.",
            },
            new()
            {
                Slug = "windsor-law", Name = "Windsor Law", ShortName = "Windsor", City = "Windsor",
                LogoUrl = "/img/schools/windsor.png", WebsiteUrl = "https://www.uwindsor.ca/law",
                AnnualTuitionCad = 22500, ClassSize = 165, MedianLsat = 156, MedianGpa = 3.6m,
                EmploymentRateAt9MonthsPct = 87, SevenSistersHireRatePct = 8, BarPassRatePct = 90,
                AboutHtml = "<p>Access-to-justice-forward curriculum, dual JD/JD with University of Detroit Mercy available.</p>",
                ClinicNotes = "Community Legal Aid clinic is unusually large and offers full clinical immersion.",
                PreLawPathwayNotes = "Holistic admissions weighting; strong intake from non-traditional backgrounds.",
            },
            new()
            {
                Slug = "lincoln-alexander", Name = "Lincoln Alexander School of Law (TMU)", ShortName = "TMU Law", City = "Toronto",
                LogoUrl = "/img/schools/tmu.png", WebsiteUrl = "https://www.torontomu.ca/law",
                AnnualTuitionCad = 26500, ClassSize = 160, MedianLsat = 158, MedianGpa = 3.65m,
                EmploymentRateAt9MonthsPct = 85, SevenSistersHireRatePct = 12, BarPassRatePct = 89,
                AboutHtml = "<p>Newest Ontario law school; experiential-learning-first curriculum with embedded technology and access-to-justice tracks.</p>",
                CareerOutcomesNotes = "Outcomes still maturing; early data suggests strong placement in tech-adjacent practices.",
                PreLawPathwayNotes = "Explicit pathway support for first-generation, equity-deserving, and non-traditional applicants.",
            },
        };
    }

    private static List<Article> BuildArticles()
    {
        var now = DateTime.UtcNow;
        return new List<Article>
        {
            new()
            {
                Slug = "2026-articling-recruit-what-changed",
                Title = "The 2026 Articling Recruit: What Actually Changed",
                Subtitle = "Three structural shifts in this year's Bay Street application cycle — and what they mean for 2L students.",
                Excerpt = "Earlier OCI deadlines, expanded Hamilton intake, and the quiet rise of AI-fluency questions in interviews.",
                BodyHtml = "<p>The 2026 articling recruit is, on the surface, structurally similar to 2025. Look closer.</p><p>OCI deadlines have shifted earlier by an average of nine days — a quiet change that disproportionately affects students relying on December GPAs. Hamilton intake at three of the Seven Sisters has expanded materially, suggesting a serious investment in regional offices for the first time in a decade. And every single one of the seven verified firms in this directory now asks an AI-fluency question in their callback interviews.</p><p>For 2L students, this means three things.</p>",
                Category = ArticleCategory.Careers,
                AuthorName = "Juris Editorial",
                PublishedAtUtc = now.AddDays(-2),
                IsFeatured = true,
                ReadTimeMinutes = 6,
            },
            new()
            {
                Slug = "the-vault-blakes-articling-week-one",
                Title = "The Vault: Inside Blakes' Week One",
                Subtitle = "Anonymous reflections from a 2025 articling student.",
                Excerpt = "\"They handed me a deal on day three. I sat in on a call with the GC of a public issuer. I had no idea what I was doing.\"",
                BodyHtml = "<p>The Vault is where students tell us what really happens. Today: the first week at Blakes from a member of the 2025 articling class.</p>",
                Category = ArticleCategory.TheVault,
                AuthorName = "Anonymous, 2025 Articling Class",
                PublishedAtUtc = now.AddDays(-5),
                ReadTimeMinutes = 4,
            },
            new()
            {
                Slug = "ontario-tuition-vs-outcomes-2026",
                Title = "Tuition vs. Outcomes: The 2026 Ontario Law School Map",
                Subtitle = "What every dollar of tuition actually buys you, by school.",
                Excerpt = "The gap between the cheapest and most expensive Ontario law schools is now $16,000 per year. The gap in median salaries on graduation is much smaller.",
                BodyHtml = "<p>This is the data the schools won't put on their viewbooks. We will.</p>",
                Category = ArticleCategory.MarketNews,
                AuthorName = "Juris Editorial",
                PublishedAtUtc = now.AddDays(-9),
                IsFeatured = true,
                ReadTimeMinutes = 8,
            },
            new()
            {
                Slug = "1l-summer-recruit-toronto-cohort-2026",
                Title = "The 1L Summer Recruit: Toronto Cohort, 2026",
                Subtitle = "Who hired, who didn't, and what the data shows about EDI-stream offers.",
                Excerpt = "Three Seven Sisters firms expanded their 1L EDI-stream programmes this year. One quietly contracted theirs.",
                BodyHtml = "<p>For the first time, all major Bay Street firms have published 1L summer offer counts.</p>",
                Category = ArticleCategory.Careers,
                AuthorName = "Juris Editorial",
                PublishedAtUtc = now.AddDays(-12),
                ReadTimeMinutes = 7,
            },
            new()
            {
                Slug = "mcmaster-pre-law-pathway",
                Title = "The McMaster-to-Law Pathway: A Field Guide",
                Subtitle = "How the Social Sciences faculty became a feeder for Ontario JD programmes.",
                Excerpt = "If you are a McMaster Social Sciences student thinking about law, this is the article we wish we'd written sooner.",
                BodyHtml = "<p>McMaster does not have a law school. Why, then, are McMaster Social Sciences students disproportionately represented in Ontario JD intakes?</p>",
                Category = ArticleCategory.StudentLife,
                AuthorName = "Juris Editorial",
                PublishedAtUtc = now.AddDays(-18),
                ReadTimeMinutes = 5,
            },
            new()
            {
                Slug = "ai-policies-by-firm-the-2026-snapshot",
                Title = "Every Bay Street Firm's AI Policy in One Table",
                Subtitle = "We asked. Some answered. Here is what we know.",
                Excerpt = "The most permissive firm allows generative AI in client-facing drafting. The most restrictive blocks all external tools at the firewall.",
                BodyHtml = "<p>Generative AI in legal practice is no longer hypothetical. Here is where each firm sits as of this quarter.</p>",
                Category = ArticleCategory.MarketNews,
                AuthorName = "Juris Editorial",
                PublishedAtUtc = now.AddDays(-25),
                ReadTimeMinutes = 6,
            },
        };
    }
}
