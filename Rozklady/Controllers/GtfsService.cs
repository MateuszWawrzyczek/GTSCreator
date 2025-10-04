using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using CsvHelper;

public class GtfsDailyService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly List<Customer> _cities = new List<Customer>
    {
        new Customer { Prefix = "wodzislaw", Name = "Wodzisław", Domain = "kiedyprzyjedzie.pl/" },
        new Customer { Prefix = "raciborz", Name = "Racibórz", Domain = "kiedyprzyjedzie.pl/" }
    };

    public GtfsDailyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(now.Hour >= 2 ? 1 : 0).AddHours(2);
            var delay = nextRun - now;
            if (delay.TotalMilliseconds > 0)
                await Task.Delay(delay, stoppingToken);

            Console.WriteLine($"[{DateTime.Now}] Start generowania GTFS dla wszystkich miast");

            using (var scope = _serviceProvider.CreateScope())
            {
                var scraperService = scope.ServiceProvider.GetRequiredService<ScraperService>();
                var generator = scope.ServiceProvider.GetRequiredService<GtfsGenerator>();

                await Parallel.ForEachAsync(_cities, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 3,
                    CancellationToken = stoppingToken
                }, async (city, ct) =>
                {
                    await GenerateAndSaveGtfsForCity(scraperService, generator, city, ct);
                });
            }

            Console.WriteLine($"[{DateTime.Now}] Zakończono generowanie GTFS dla wszystkich miast");
        }
    }

    private async Task GenerateAndSaveGtfsForCity(
        ScraperService scraperService,
        GtfsGenerator generator,
        Customer city,
        CancellationToken ct)
    {
        try
        {
            var data = await scraperService.RunScrapingPipelineAsync(city);
            if (data == null)
            {
                Console.WriteLine($"Brak danych dla {city.Name}");
                return;
            }

            var gtfsBytes = await generator.GenerateGtfsAsync(data);

            // Ustal folder docelowy
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeneratedGtfs");
            Directory.CreateDirectory(folder); // utwórz jeśli nie istnieje

            var fileName = Path.Combine(folder, $"{city.Prefix}_gtfs.zip");

            await File.WriteAllBytesAsync(fileName, gtfsBytes, ct);
            Console.WriteLine($"GTFS wygenerowane dla {city.Name} w {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd przy generowaniu GTFS dla {city.Name}: {ex.Message}");
        }
    }
}