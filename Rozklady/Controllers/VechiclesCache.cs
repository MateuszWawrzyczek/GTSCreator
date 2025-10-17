using Rozklady.Models;
using System;
using System.Collections.Generic;

public static class VehicleCache
{
    private static List<VehicleDto> _cache = new();
    private static DateTime _lastFetch = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);

    public static List<VehicleDto> GetCache() => _cache;

    public static bool IsCacheValid() => DateTime.UtcNow - _lastFetch < CacheDuration;

    public static void SetCache(List<VehicleDto> vehicles)
    {
        _cache = vehicles;
        _lastFetch = DateTime.UtcNow;
    }
}
