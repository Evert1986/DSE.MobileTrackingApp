using System.Net.Http.Json;
using DSE.MobileTrackingApp.Models;

namespace DSE.MobileTrackingApp.Services;

public sealed class ApiTrackingDataService : ITrackingDataService
{
    private readonly HttpClient _httpClient;

    private List<BasicMachineValuesDto>? _cachedValues;
    private DateTime _lastLoadedUtc;

    public ApiTrackingDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrentRun> GetCurrentRunAsync()
    {
        var values = await GetLatestValuesAsync();

        var latest = values
            .Where(x => x.DateOfLog.HasValue)
            .OrderByDescending(x => x.DateOfLog)
            .FirstOrDefault();

        return new CurrentRun(
            Facility: "Demo Facility",
            Packline: latest is null ? "Live API" : $"{latest.MachineType} Line {latest.Line}",
            Variety: "Live SQL Data",
            BatchId: latest?.MachineName ?? "-",
            OperatorName: "Azure SQL API",
            StartTime: latest?.DateOfLog ?? DateTime.Now,
            IsRunning: latest is not null
        );
    }

    public async Task<List<ParameterReading>> GetParameterReadingsAsync()
    {
        var values = await GetLatestValuesAsync();

        var readings = new List<ParameterReading>();

        foreach (var item in values)
        {
            var prefix = $"{item.MachineName}";

            AddReading(readings, $"{prefix} pH", "💧", item.PH, "", 5.0m, 7.0m);
            AddReading(readings, $"{prefix} Temperature", "🌡️", item.Temperature, "°C", 18.0m, 24.0m);
            AddReading(readings, $"{prefix} Tons/hr", "⚖️", item.TonsPerHour, "t/h", 0m, 100m);
            AddReading(readings, $"{prefix} Bin Count", "📦", item.BinCountSinceLastReset, "bins", 0m, 999999m);
        }

        return readings;
    }

    public async Task<List<HistoryReading>> GetHistoryAsync()
    {
        var values = await GetLatestValuesAsync();

        var history = new List<HistoryReading>();

        foreach (var item in values)
        {
            var readingTime = item.DateOfLog ?? DateTime.Now;
            var prefix = $"{item.MachineName}";

            AddHistory(history, $"{prefix} pH", "💧", item.PH, "", readingTime, 5.0m, 7.0m);
            AddHistory(history, $"{prefix} Temperature", "🌡️", item.Temperature, "°C", readingTime, 18.0m, 24.0m);
            AddHistory(history, $"{prefix} Tons/hr", "⚖️", item.TonsPerHour, "t/h", readingTime, 0m, 100m);
            AddHistory(history, $"{prefix} Bin Count", "📦", item.BinCountSinceLastReset, "bins", readingTime, 0m, 999999m);
        }

        return history
            .OrderByDescending(x => x.ReadingTime)
            .ToList();
    }

    public async Task<List<TrackingAlert>> GetAlertsAsync()
    {
        var values = await GetLatestValuesAsync();

        var alerts = new List<TrackingAlert>();

        foreach (var item in values)
        {
            if (!item.DateOfLog.HasValue)
            {
                alerts.Add(new TrackingAlert(
                    Title: $"{item.MachineName} No Data",
                    Message: "No latest SQL data was returned for this machine.",
                    Severity: "warning",
                    Time: DateTime.Now
                ));
            }
        }

        return alerts;
    }

    public Task SaveReadingAsync(ReadingInput input)
    {
        // We will connect this to a POST endpoint later.
        return Task.CompletedTask;
    }

    private async Task<List<BasicMachineValuesDto>> GetLatestValuesAsync()
    {
        if (_cachedValues is not null && DateTime.UtcNow.Subtract(_lastLoadedUtc).TotalSeconds < 5)
        {
            return _cachedValues;
        }

        var values = await _httpClient.GetFromJsonAsync<List<BasicMachineValuesDto>>(
            "api/mobile/latest-basic-values");

        _cachedValues = values ?? new List<BasicMachineValuesDto>();
        _lastLoadedUtc = DateTime.UtcNow;

        return _cachedValues;
    }

    private static void AddReading(
        List<ParameterReading> readings,
        string name,
        string icon,
        decimal? value,
        string unit,
        decimal targetMin,
        decimal targetMax)
    {
        if (!value.HasValue)
        {
            return;
        }

        readings.Add(new ParameterReading(
            name,
            icon,
            value.Value,
            unit,
            targetMin,
            targetMax,
            value.Value >= targetMin && value.Value <= targetMax
        ));
    }

    private static void AddHistory(
        List<HistoryReading> history,
        string parameter,
        string icon,
        decimal? value,
        string unit,
        DateTime readingTime,
        decimal targetMin,
        decimal targetMax)
    {
        if (!value.HasValue)
        {
            return;
        }

        history.Add(new HistoryReading(
            parameter,
            icon,
            value.Value,
            unit,
            readingTime,
            value.Value >= targetMin && value.Value <= targetMax
        ));
    }
}