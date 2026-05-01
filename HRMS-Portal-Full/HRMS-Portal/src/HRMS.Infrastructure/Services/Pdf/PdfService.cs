using HRMS.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace HRMS.Infrastructure.Services.Pdf;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    // ? Shared browser instance (IMPORTANT)
    private static IBrowser? _browser;
    private static readonly SemaphoreSlim _browserLock = new(1, 1);
    private static bool _isChromiumDownloaded = false;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    // ? Centralized browser creation
    private async Task<IBrowser> GetBrowserAsync(CancellationToken ct)
    {
        if (_browser != null && _browser.IsConnected)
            return _browser;

        await _browserLock.WaitAsync(ct);
        try
        {
            if (_browser == null || !_browser.IsConnected)
            {
                _logger.LogInformation("Initializing Puppeteer browser...");

                // ? FIX: Ensure Chromium is downloaded
                if (!_isChromiumDownloaded)
                {
                    var fetcher = new BrowserFetcher();
                    var revision = await fetcher.DownloadAsync();
                    _logger.LogInformation("Chromium downloaded at: {Path}", revision.GetExecutablePath);
                    _isChromiumDownloaded = true;
                }

                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage"
                    }
                });
            }

            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }

    public async Task<byte[]> GeneratePdfAsync(string htmlContent, string title = "", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Generating PDF: {Title}", title);

            var browser = await GetBrowserAsync(ct);

            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdf = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "20px",
                    Bottom = "20px",
                    Left = "20px",
                    Right = "20px"
                }
            });

            return pdf;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed: {Title}", title);
            throw new InvalidOperationException($"PDF generation failed: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GenerateSalarySlipPdfAsync(object model, CancellationToken ct = default)
    {
        // ?? FIX: Don't use ToString() for HTML
        var htmlContent = model?.ToString() ?? "<html><body><h1>No Data</h1></body></html>";

        return await GeneratePdfAsync(htmlContent, "SalarySlip", ct);
    }
}


//using HRMS.Application.Interfaces.Services;
//using Microsoft.Extensions.Logging;
//using PuppeteerSharp;
//using PuppeteerSharp.Media;

//namespace HRMS.Infrastructure.Services.Pdf;

//public class PdfService : IPdfService
//{
//    private readonly ILogger<PdfService> _logger;
//    private static readonly SemaphoreSlim _lock = new(1, 1);

//    public PdfService(ILogger<PdfService> logger) { _logger = logger; }

//    public async Task<byte[]> GeneratePdfAsync(string htmlContent, string title = "", CancellationToken ct = default)
//    {
//        await _lock.WaitAsync(ct);
//        try
//        {
//            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
//            {
//                Headless = true,
//                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
//            });
//            using var page = await browser.NewPageAsync();
//            await page.SetContentAsync(htmlContent,
//                new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
//            return await page.PdfDataAsync(new PdfOptions
//            {
//                Format = PaperFormat.A4, PrintBackground = true,
//                MarginOptions = new MarginOptions { Top = "20px", Bottom = "20px", Left = "20px", Right = "20px" }
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "PDF generation failed: {Title}", title);
//            throw new InvalidOperationException($"PDF generation failed: {ex.Message}", ex);
//        }
//        finally { _lock.Release(); }
//    }

//    public async Task<byte[]> GenerateSalarySlipPdfAsync(object model, CancellationToken ct = default)
//        => await GeneratePdfAsync(model.ToString() ?? string.Empty, "SalarySlip", ct);
//}
