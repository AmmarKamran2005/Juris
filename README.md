# Juris — The Legal Intelligence Hub

Production-ready data platform for Ontario law students and the firms that hire them. Built on **ASP.NET Core 8 + Blazor Web App** with **SQL Server**, **EF Core**, and **ASP.NET Core Identity**.

> Vision: the "Legal Cheek" of Canada — a definitive, data-driven career and media platform, starting with a 2026 Ontario-first launch.

---

## What's inside

### Public site (no auth required)
| Route | Purpose |
|---|---|
| `/` | Hero search, live deadline ticker, featured partners, trending insights, newsletter |
| `/firms` | Firm Directory — sticky sidebar (City / Practice Area / Salary range / Peer group / Verified-only) + sortable card grid + pagination |
| `/firms/{slug}` | Firm profile — split-screen: **Hard Data** sidebar + **Vibe** main. Every section instrumented for analytics. |
| `/schools` | Comparison matrix with **per-row checkboxes** + card grid |
| `/schools/compare?slugs=a,b,c` ·or· `?s=a&s=b&s=c` | Side-by-side data comparison |
| `/schools/{slug}` | Deep-dive (mooting, clinics, Pre-Law pathways, career outcomes) |
| `/recruitment` | Live tracker — type & status filters, countdowns, direct apply links |
| `/recruitment/feed.ics` | Public **iCal** feed of upcoming deadlines (subscribe in Google Calendar / Outlook / Apple) |
| `/news` | News & Insights — Careers / Market News / Student Life / The Vault categories |
| `/news/{slug}` | Article with read-time analytics |
| `/search?q=…` | Cross-entity search (firms, schools, articles) |
| `/verified` | Marketing landing page for firms (verified portal pitch) |
| `/about` | Editorial about, contact, privacy |
| `/sitemap.xml`, `/robots.txt` | SEO |
| `/health`, `/health/ready` | Liveness / readiness for orchestrators |

### Verified Portal (FirmHR role required)
| Route | Purpose |
|---|---|
| `/portal` | HR home — quick stats + entry points |
| `/portal/firm` | Edit firm profile + **inline CRUD** for Practice Areas, Perks, Deadlines, Reviews |
| `/portal/analytics` | **HR Analytics Dashboard** — profile views (30-day SVG sparkline), CTR, dwell, top universities, year-of-study breakdown, Vibe Heatmap, peer benchmark, drop-off analysis, top search terms |

### Admin (Admin role required)
| Route | Purpose |
|---|---|
| `/admin` | Global dashboard — KPIs, 30-day events sparkline, top search terms, exports |
| `/admin/firms` + `/admin/firms/{id}` | List + edit any firm; toggle Verified / Featured / Published; **inline editor for Perks / Practice Areas / Deadlines / Reviews** |
| `/admin/schools` + `/admin/schools/{id}` | School CRUD |
| `/admin/articles` + `/admin/articles/{id}` | News CRUD with HTML body + auto-slug button |
| `/admin/users` + `/admin/users/{id}` | List, create FirmHR, **edit any user** (profile, roles, password reset, delete) |
| `/admin/subscribers` | Newsletter subscriber list with active/unsub counts |
| `/admin/import` | **CSV bulk import** for firms/schools/articles with idempotent slug-matching & overwrite toggle |

### Exports (Admin only)
| Endpoint | Returns |
|---|---|
| `/admin/import/template?entity=firms\|schools\|articles` | CSV header template with sample row |
| `/admin/export/firms.csv` | Full firm dataset including pipe-delimited PracticeAreas |
| `/admin/export/subscribers.csv` | Newsletter list |
| `/admin/export/analytics.csv?from=&to=` | Raw analytics events for date range (default last 30 days) |

### API
| Endpoint | Purpose |
|---|---|
| `POST /api/analytics/track` | JSON ingestion endpoint, **rate-limited 120 req/min/IP** (called by `/wwwroot/js/juris.js` via `navigator.sendBeacon`) |

---

## Architecture (Clean Architecture)

```
Juris.sln
├── src/
│   ├── Juris.Domain/            ← Entities + Enums (Firm, School, Article, AnalyticsEvent, …)
│   ├── Juris.Application/       ← Service interfaces + concrete services + DTOs
│   │                              FirmService · SchoolService · ArticleService
│   │                              RecruitmentService · SearchService
│   │                              NewsletterService · AnalyticsService · CsvImportService
│   │                              IApplicationDbContext abstraction
│   ├── Juris.Infrastructure/    ← EF Core ApplicationDbContext, migrations, ApplicationUser, Roles
│   ├── Juris.Web/               ← Blazor Web App (SSR + Interactive Auto)
│   │                              + Account/Identity components (custom-styled Login)
│   │                              + DatabaseInitializer + SeedData
│   │                              + AnalyticsEndpoints + AdminUtilityEndpoints + PublicEndpoints
│   └── Juris.Web.Client/        ← WebAssembly client (interactive components)
└── tests/Juris.Tests/
```

### Tech stack
| Concern | Choice |
|---|---|
| Frontend | Blazor Web App, .NET 8, Interactive Auto rendering |
| Backend | ASP.NET Core 8 |
| Database | SQL Server 2019+ (works on 2019/2022/2025) |
| ORM | EF Core 8.0.25 |
| Auth | ASP.NET Core Identity, cookie-based, role policies (`Admin`, `FirmHR`, `Subscriber`) |
| Analytics | Event-sourced `AnalyticsEvent` table with hot indexes; aggregated on demand |
| CSV import | `CsvHelper` 33 — idempotent slug-matching, overwrite toggle, partial-row tolerance |
| Calendar export | Hand-rolled iCal (RFC 5545) — no external lib |
| Charts | Inline SVG sparkline + bar list (no JS chart lib dependency) |
| Rate limiting | ASP.NET Core 8 built-in `RateLimiter` middleware (`FixedWindow` per IP) |
| Fonts | Inter (sans) + Lora (serif), loaded from Google Fonts |

### Design system
Strict monochrome — `#FFFFFF` bg, `#000000` text, `#F2F2F2` dividers. Sharp corners (`border-radius: 0`), `1px solid black` borders, no shadows, no gradients. Inter sans for everything UI; Lora serif for body prose. See [`src/Juris.Web/wwwroot/app.css`](src/Juris.Web/wwwroot/app.css).

---

## Running locally

### Prerequisites
- .NET 8 SDK (8.0.4xx LTS) — `dotnet --version`
- SQL Server 2019+ on `localhost` with Integrated Security (Windows Auth)

`global.json` pins the SDK to .NET 8 LTS.

### Setup
```bash
cd "C:\Main\Juris"
dotnet restore
dotnet build Juris.sln
```

### Apply database migrations
On startup, `DatabaseInitializer` runs `MigrateAsync` automatically — but you can also do it manually:
```bash
dotnet ef database update --project src/Juris.Infrastructure --startup-project src/Juris.Web
```

### Run
```bash
dotnet run --project src/Juris.Web --urls=http://localhost:5099
```

Open <http://localhost:5099/>.

The first start:
1. Applies pending migrations to `JurisDb`
2. Creates roles `Admin`, `FirmHR`, `Subscriber`
3. Creates default admin user (configured in `appsettings.json`)
4. Seeds 12 placeholder firms, 7 schools, 6 articles, ~30 perks, ~20 deadlines

Subsequent starts are idempotent — seed data only runs if tables are empty.

### Default admin login
| Email | Password |
|---|---|
| `admin@juris.local` | `ChangeMe!2026` |

**⚠️ Change this in `appsettings.json` (`JurisAdmin` section) before deploying.**

---

## Deployment

### IIS (Windows Server)
1. Publish: `dotnet publish src/Juris.Web -c Release -o ./publish`
2. Install [.NET 8 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on the server.
3. Create an IIS site pointing to the `publish` folder.
4. Set the App Pool to "No Managed Code" (ASP.NET Core uses an out-of-process model).
5. Configure SQL Server connection string in `appsettings.Production.json` or via env var:
   ```bash
   ConnectionStrings__DefaultConnection="Server=...;Database=JurisDb;..."
   JurisAdmin__Password="<a strong production password>"
   ```
6. Grant the App Pool identity (or a dedicated SQL login) `db_owner` on `JurisDb`.

### Linux + systemd (Kestrel + reverse proxy)
1. Publish: `dotnet publish src/Juris.Web -c Release -o /var/www/juris -r linux-x64 --self-contained false`
2. Install .NET 8 runtime: `apt install dotnet-runtime-8.0 aspnetcore-runtime-8.0`
3. Create `/etc/systemd/system/juris.service`:
   ```ini
   [Unit]
   Description=Juris
   After=network.target
   [Service]
   WorkingDirectory=/var/www/juris
   ExecStart=/usr/bin/dotnet /var/www/juris/Juris.Web.dll
   Restart=always
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5099
   [Install]
   WantedBy=multi-user.target
   ```
4. Reverse-proxy via nginx/Apache to `localhost:5099`. Terminate TLS at the proxy.

### Docker
The repo ships with a production-ready multi-stage `Dockerfile` at the root —
just `docker build -t juris .` and `docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." juris`.

### Render (Postgres + free tier)
The repo ships with `render.yaml` blueprint — push to GitHub, then on Render:
1. **Dashboard → New → Blueprint** → connect this repo
2. Render reads `render.yaml`, provisions:
   - Postgres free tier (90-day free, then $7/mo)
   - Web service (Docker, free tier — sleeps after 15min idle)
   - Connection string auto-injected; admin password auto-generated
3. First deploy takes ~5min (Docker build + EF schema create + seed)
4. Get admin password from Render dashboard → **Environment** → `JurisAdmin__Password`

**App auto-detects provider** — if connection string is a `postgres://…` URL it switches to Npgsql; otherwise SQL Server. So the same codebase runs on Windows + SQL Server locally AND on Render with managed Postgres without any code changes.

### Production checklist
- [ ] `JurisAdmin:Password` rotated and removed from `appsettings.json` (set via env var instead)
- [ ] `ConnectionStrings:DefaultConnection` points to production SQL with a least-privilege login
- [ ] HTTPS termination configured at the load balancer / reverse proxy; HSTS enabled (already set in Program.cs for non-Development)
- [ ] `Smtp` block populated when newsletter goes live
- [ ] Logging shipped to a sink (`Microsoft.Extensions.Logging` → Serilog/Seq/Application Insights as needed)
- [ ] Backup schedule on SQL Server (full + transaction-log)
- [ ] Health probes pointed at `/health` (liveness) and `/health/ready` (readiness — checks DB)
- [ ] CDN / cache fronting `/` `/firms` `/schools` `/news/*` (15-min TTL is safe; analytics endpoint stays uncached)

---

## Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=JurisDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "JurisAdmin": {
    "Email": "admin@juris.local",
    "Password": "ChangeMe!2026",
    "DisplayName": "Juris Admin"
  },
  "Smtp": { "...": "placeholder for future SMTP outbound mail" }
}
```

Override per-environment with `appsettings.Production.json` or env vars (e.g., `JurisAdmin__Password`).

---

## Provisioning a verified firm

End-to-end flow (no email/SMTP required):

1. Sign in as admin (`admin@juris.local`) at `/Account/Login`.
2. Open **Admin → Manage Users** (`/admin/users`).
3. Fill the "Provision FirmHR account" form: email, display name, initial password (≥8 chars, mixed case + digit), and select the firm to link.
4. Submit. The system:
   - Creates the user
   - Assigns the `FirmHR` role
   - Sets `Firm.OwnerUserId` to the new user's ID
   - Flips `Firm.IsVerified = true`
5. Share the email + password with the firm contact. They sign in at `/Account/Login` and land on `/portal`.
6. From `/portal/firm` they can edit basic fields **plus inline-manage** their Practice Areas, Perks, Deadlines, and Reviews.

---

## CSV Bulk Import

The PDF blueprint specifies the platform must accept "a CSV/spreadsheet of 200+ firms and schools." That's wired:

1. **Admin → CSV Bulk Import** (`/admin/import`).
2. Pick entity type (Firms / Schools / Articles).
3. Choose **Overwrite existing rows** if you want to re-import & update, or leave unchecked to skip already-present slugs.
4. Upload a UTF-8 CSV. The endpoint also handles partial rows (missing headers are tolerated).
5. The result panel reports `Created / Updated / Skipped / Errors`.

### Templates
Click "Download CSV template" on the import page, or hit:
- `/admin/import/template?entity=firms`
- `/admin/import/template?entity=schools`
- `/admin/import/template?entity=articles`

Each template includes the full header row plus one example data row.

### Exporting current data
- `/admin/export/firms.csv` — round-trippable with the firm template (PracticeAreas pipe-delimited).
- `/admin/export/subscribers.csv` — for marketing-tool sync.
- `/admin/export/analytics.csv?from=2026-04-01&to=2026-05-01` — raw events for ad-hoc analysis.

---

## How the Analytics Engine works

Every meaningful interaction emits an event into the `AnalyticsEvents` table:

| Event | Captured by |
|---|---|
| `PageView` | `juris.js` on `DOMContentLoaded` |
| `FirmProfileView`, `SchoolProfileView`, `ArticleView` | `data-track-click` attributes on cards + server-side track on detail pages |
| `ApplyButtonClick`, `EarlyCareersClick`, `DeadlineClick` | `data-track-click` on each link |
| `PerkClick` | Each perk cell on a firm profile |
| `SectionDwell` | IntersectionObserver on `[data-track-section]` regions; emits dwell-seconds when scrolled out of view |
| `SearchQuery` | Server-side track in `/search` |
| `NewsletterSignup` | Server-side track on form submit |

**Privacy:** anonymous session IDs (UUIDv4 in localStorage), IP addresses are SHA-256 hashed at ingestion, university affiliation and year-of-study are voluntarily provided through the newsletter signup or browser localStorage (`juris.setIdentity`). Endpoint is rate-limited to 120 events / minute / IP.

The HR Dashboard aggregates these events on demand to render every chart in the verified portal.

---

## Database management

| Task | Command |
|---|---|
| Add a migration | `dotnet ef migrations add <Name> --project src/Juris.Infrastructure --startup-project src/Juris.Web` |
| Apply migrations | `dotnet ef database update --project src/Juris.Infrastructure --startup-project src/Juris.Web` |
| Roll back | `dotnet ef database update <PreviousMigrationName> --project src/Juris.Infrastructure --startup-project src/Juris.Web` |
| Drop the database | `dotnet ef database drop --project src/Juris.Infrastructure --startup-project src/Juris.Web --force` |

---

## Roadmap (future)

- [ ] SMTP outbound (the `Smtp` config block + `MailKit` integration). Newsletter confirmation + admin alerts.
- [ ] Hangfire (or hosted services) for daily aggregation rollups + scheduled email digests.
- [ ] Real firm/school logos (drop into `wwwroot/img/firms/{slug}.png` and `wwwroot/img/schools/{slug}.png` — fallback to letter-glyph already works).
- [ ] Email confirmations + password reset email (currently `RequireConfirmedAccount = false` for dev convenience).
- [ ] Rich-text article editor (currently HTML textarea — works, but a TipTap/CKEditor upgrade would help editorial).
- [ ] Image uploads to local file storage / Azure Blob / S3.
- [ ] Unit + integration tests (xUnit project is scaffolded; expand coverage).
- [ ] Multi-language support (the platform is Ontario-first but Quebec-bilingual coverage is on the table).

---

## License & contact

Internal project — `partners@juris.local` (placeholder).
