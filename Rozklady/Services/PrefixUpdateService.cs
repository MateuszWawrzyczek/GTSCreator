using Microsoft.EntityFrameworkCore;
using Rozklady.Data;

public class PrefixService
{
    private readonly IDbContextFactory<RozkladyContext> _contextFactory;

    public int? MzkPrefix { get; private set; }
    public int? KmrPrefix { get; private set; }

    public PrefixService(IDbContextFactory<RozkladyContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task RefreshPrefixesAsync()
    {
        await using var db = _contextFactory.CreateDbContext();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        
        MzkPrefix = await db.CalendarDates
            .Where(cd => cd.FeedId == "MZK" && cd.Date == today)
            .Select(cd => (int?)int.Parse(cd.ServiceId.Substring(0, 4)))
            .FirstOrDefaultAsync();


        KmrPrefix = await db.CalendarDates
            .Where(cd => cd.FeedId == "KMR" && cd.Date == today)
            .Select(cd => (int?)int.Parse(cd.ServiceId.Substring(0, 4)))
            .FirstOrDefaultAsync();

        Console.WriteLine($"[PrefixService] Prefixy zaktualizowane: MZK={MzkPrefix}, KMR={KmrPrefix}");
    }
}


public class PrefixUpdateService : BackgroundService
{
    private readonly IServiceProvider _services;

    public PrefixUpdateService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddHours(11).AddMinutes(13); 
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);

            using var scope = _services.CreateScope();
            var prefixService = scope.ServiceProvider.GetRequiredService<PrefixService>();
            await prefixService.RefreshPrefixesAsync();
        }
    }
}
