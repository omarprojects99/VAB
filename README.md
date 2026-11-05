# VAB

VAB is an ASP.NET Core MVC application for managing airline catering operations. It brings together sales and offer records, catering comparisons, cabin crew commissions, cash-shortage tracking, and reporting.

## Main features

- Role-based sign-in and user management with ASP.NET Core Identity
- Management of articles, categories, prices, sales, and catering offers
- Comparison views and reports for catering sales and offers
- Commission and cash-shortage calculations and dashboards
- PDF extraction and OCR for scanned documents
- Excel export, live notifications, and optional SMS notifications

The interface is primarily in French.

## Technology

- .NET 8 and ASP.NET Core MVC
- Entity Framework Core with SQL Server and EF Core migrations
- ASP.NET Core Identity and SignalR
- PdfPig and Tesseract for PDF processing and OCR
- ClosedXML for Excel exports
- Twilio for optional SMS notifications

## Run locally

Requirements: .NET 8 SDK and a SQL Server instance. The checked-in development connection string uses a local `SQLEXPRESS` instance with Windows authentication; update it if your SQL Server setup differs.

From the repository root:

1. Restore and build the solution:

   ```powershell
   dotnet restore .\VAB\VAB.sln
   dotnet build .\VAB\VAB.sln --no-restore
   ```

2. Set up the database connection using User Secrets:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=VAB;Trusted_Connection=True;TrustServerCertificate=True" --project .\VAB\VAB.csproj
   ```

3. If Entity Framework tools are not installed, install the matching .NET 8 tool and apply the migrations:

   ```powershell
   dotnet tool install --global dotnet-ef --version 8.0.14
   dotnet ef database update --project .\VAB\VAB.csproj
   ```

4. To create a development administrator, set both values in User Secrets:

   ```powershell
   dotnet user-secrets set "AdminBootstrap:Email" "you@example.com" --project .\VAB\VAB.csproj
   dotnet user-secrets set "AdminBootstrap:Password" "<choose-a-strong-password>" --project .\VAB\VAB.csproj
   ```

   The bootstrap runs only in the Development environment. SMS is optional; to enable it, set `Twilio:AccountSid`, `Twilio:MessagingServiceSid`, and `Twilio:AuthToken` in User Secrets.

5. Start the application:

   ```powershell
   dotnet run --project .\VAB\VAB.csproj
   ```

For deployment, supply settings through the hosting environment rather than committing secrets. ASP.NET Core maps names such as `ConnectionStrings__DefaultConnection` and `Twilio__AuthToken` to their corresponding configuration keys.

## Notes

- OCR uses the English and French Tesseract language data under `VAB\Helpers\tessdata`.
- No automated test project is currently included in the solution.
