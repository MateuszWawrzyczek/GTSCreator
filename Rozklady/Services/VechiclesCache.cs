public static class VehicleCache
{
    private static List<VehicleDto> _cache = new();
    private static readonly object _lock = new();

    public static List<VehicleDto> GetCache()
    {
        lock (_lock)
        {
            return _cache.ToList(); 
        }
    }

    public static void UpdateCache(List<VehicleDto> vehicles)
    {
        lock (_lock)
        {
            _cache = vehicles;
        }
    }
}
