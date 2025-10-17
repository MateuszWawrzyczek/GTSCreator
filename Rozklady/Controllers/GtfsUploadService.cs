using System.IO.Compression;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Rozklady.Data;

public class GtfsUploader
{
    private readonly IDbContextFactory<RozkladyContext> _dbFactory;

    public GtfsUploader(IDbContextFactory<RozkladyContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task UploadGtfsToDbAsync(string feedId, byte[] gtfsZip)
    {
        using var db = _dbFactory.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var deleteOrder = new[] { "stop_times", "trips", "routes", "stops", "calendar_dates", "calendar", "agency" };
            foreach (var table in deleteOrder)
                #pragma warning disable EF1002
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM {table} WHERE feed_id = {{0}};", feedId);
                #pragma warning restore EF1002

            using var memStream = new MemoryStream(gtfsZip);
            using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);

            var importOrder = new[] { "agency", "calendar", "calendar_dates", "routes", "stops", "trips", "stop_times" };

            foreach (var tableName in importOrder)
            {
                var entry = archive.Entries.FirstOrDefault(e => 
                    Path.GetFileNameWithoutExtension(e.Name).Equals(tableName, StringComparison.OrdinalIgnoreCase));
                if (entry == null) continue;

                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<dynamic>().ToList();
                if (!records.Any())
                    continue;

                var columnNames = ((IDictionary<string, object>)records.First()).Keys.ToList();
                columnNames.Add("feed_id");

                var sql = $"INSERT INTO {tableName} ({string.Join(",", columnNames)}) VALUES ";
                var valuesList = new List<string>();

                foreach (var record in records)
                {
                    var values = ((IDictionary<string, object>)record)
                        .Select(kv => kv.Value == null ? "NULL" : $"'{kv.Value.ToString().Replace("'", "''")}'")
                        .ToList();
                    values.Add($"'{feedId}'");
                    valuesList.Add($"({string.Join(",", values)})");
                }

                sql += string.Join(",", valuesList) + ";";
                await db.Database.ExecuteSqlRawAsync(sql);
                Console.WriteLine($"✅ Wczytano {records.Count} rekordów do {tableName}");
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
