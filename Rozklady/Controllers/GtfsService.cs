using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class GtfsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GtfsBackgroundService> _logger;

    public GtfsBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GtfsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var now = DateTime.Now;
        var nextRun = now.Date.AddHours(11).AddMinutes(17);
        if (now > nextRun)
            nextRun = nextRun.AddDays(1);

        var delay = nextRun - now;
        _logger.LogInformation("Czekam do {nextRun}", nextRun);

        await Task.Delay(delay, stoppingToken);

        try
        {
            var cities = new List<Customer>
            {
                new() { Prefix = "wodzislaw", Name = "Wodzisław", Domain = "kiedyprzyjedzie.pl" },
                new() { Prefix = "pksraciborz", Name = "PKS Racibórz", Domain = "kiedyprzyjedzie.pl" },
                //new() { Prefix = "pszczyna", Name = "Pszczyna", Domain = "kiedyprzyjedzie.pl" },
                new() { Prefix = "powiatwodzislawski", Name = "Powiat Wodzisławski", Domain = "kiedyprzyjedzie.pl" },
                new() { Prefix = "raciborz", Name = "Racibórz", Domain = "kiedyprzyjedzie.pl" }
            };

            _logger.LogInformation("Start generowania GTFS o {time}", DateTime.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var scraperService = scope.ServiceProvider.GetRequiredService<ScraperService>();
                var generator = scope.ServiceProvider.GetRequiredService<GtfsGenerator>();
                var uploader = scope.ServiceProvider.GetRequiredService<GtfsUploader>();

                await Parallel.ForEachAsync(cities, new ParallelOptions { MaxDegreeOfParallelism = 3 }, async (city, ct) =>
                {
                    try
                    {
                        var data = await scraperService.RunScrapingPipelineAsync(city);
                        if (data != null)
                        {
                            var gtfsBytes = await generator.GenerateGtfsAsync(data);
                            _logger.LogInformation("✅ Wygenerowano GTFS dla {city}", city.Name);

                            var outputDir = Path.Combine(AppContext.BaseDirectory, "GeneratedGtfs");
                            Directory.CreateDirectory(outputDir);

                            var filePath = Path.Combine(outputDir, $"{city.Prefix}_gtfs_{DateTime.Now:yyyyMMdd_HHmm}.zip");
                            await File.WriteAllBytesAsync(filePath, gtfsBytes);
                            _logger.LogInformation("💾 Zapisano GTFS do pliku: {path}", filePath);

                            await uploader.UploadGtfsToDbAsync(city.Prefix, gtfsBytes);
                            _logger.LogInformation("📤 Załadowano GTFS do bazy dla {city}", city.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Błąd podczas przetwarzania {city}", city.Name);
                    }
                });
            }

            _logger.LogInformation("✅ Zakończono generowanie i upload GTFS.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Błąd w zadaniu GTFS");
        }

        // Odczekaj 24h do kolejnego uruchomienia
        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
    }
}
}