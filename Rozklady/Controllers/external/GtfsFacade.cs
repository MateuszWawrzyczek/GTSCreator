public class GtfsFacade
{
    private readonly ScraperService _scraper;
    private readonly GtfsGenerator _generator;

    public GtfsFacade(ScraperService scraper, GtfsGenerator generator)
    {
        _scraper = scraper;
        _generator = generator;
    }

    public async Task<byte[]?> GenerateGtfsForProviderAsync(Customer provider)
    {
        var scraped = await _scraper.RunScrapingPipelineAsync(provider);
        if (scraped == null) return null;
        return await _generator.GenerateGtfsAsync(scraped);
    }
}
