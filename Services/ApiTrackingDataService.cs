using System.Net.Http.Json;
using DSE.MobileTrackingApp.Models;

namespace DSE.MobileTrackingApp.Services;



public sealed class ApiTrackingDataService : ITrackingDataService
{
    private readonly HttpClient _httpClient;

    private List<BasicMachineValuesDto>? _cachedValues;
    private DateTime _lastLoadedUtc;

    public int SelectedLine { get; set; } = 1;

    public ApiTrackingDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrentRun> GetCurrentRunAsync()
    {
        var currentRun = await _httpClient.GetFromJsonAsync<CurrentRunDto>(
            $"api/mobile/current-run?line={SelectedLine}");

        if (currentRun is null)
        {
            return new CurrentRun(
                Facility: "BBI",
                Packline: $"Packline {SelectedLine}",
                Variety: "No Data",
                BatchId: "-",
                OperatorName: "David M",
                StartTime: DateTime.Now,
                IsRunning: false
            );
        }

        return new CurrentRun(
            Facility: currentRun.Facility,
            Packline: currentRun.Packline,
            Variety: currentRun.Variety,
            BatchId: currentRun.BatchId,
            OperatorName: currentRun.OperatorName,
            StartTime: currentRun.StartTime,
            IsRunning: currentRun.IsRunning
        );
    }

    public async Task<List<ParameterReading>> GetParameterReadingsAsync()
    {
        var values = await GetLatestValuesAsync();

        var readings = new List<ParameterReading>();

        foreach (var item in values)
        {
            if (IsDosingMachine(item))
            {
                AddReading(readings, "pH", "💧", item.PH, "", 5.0m, 7.0m);
                AddReading(readings, "Temperature", "🌡️", item.Temperature, "°C", 18.0m, 24.0m);
                AddReading(readings, "Tons", "⚖️", item.TonsPerHour, "t/h", 0m, 100m);
            }
            else if (IsDrenchDosingMachine(item))
            {
                AddReading(readings, "Bins", "📦", item.BinCountSinceLastReset, "bins", 0m, 999999m);
            }
            else if (IsWaxMachine(item))
            {
                AddReading(readings, "Wax", "🛢️", item.LitresPerTon, "L/Ton", 0.8m, 1.5m);
            }
            else if (IsAppData(item))
            {
                AddReading(readings, "Titration", "🧪", item.Titration, "ppm", 500m, 600m);
            }
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

            if (IsDosingMachine(item))
            {
                AddHistory(history, "pH", "💧", item.PH, "", readingTime, 5.0m, 7.0m);
                AddHistory(history, "Temperature", "🌡️", item.Temperature, "°C", readingTime, 18.0m, 24.0m);
                AddHistory(history, "Tons", "⚖️", item.TonsPerHour, "t/h", readingTime, 0m, 100m);
            }
            else if (IsDrenchDosingMachine(item))
            {
                AddHistory(history, "Bins", "📦", item.BinCountSinceLastReset, "bins", readingTime, 0m, 999999m);
            }
            else if (IsWaxMachine(item))
            {
                AddHistory(history, "Wax", "🛢️", item.LitresPerTon, "L/Ton", readingTime, 0.8m, 1.5m);
            }
            else if (IsAppData(item))
            {
                AddHistory(history, "Titration", "🧪", item.Titration, "ppm", readingTime, 500m, 600m);
            }

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

    public async Task SaveReadingAsync(ReadingInput input)
    {
        if (!input.Value.HasValue)
        {
            return;
        }

        var field = NormalizeAppDataField(input.Parameter);

        var request = new AppDataInputDto
        {
            Line = SelectedLine,
            Field = field,
            Value = input.Value.Value
        };

        var response = await _httpClient.PostAsJsonAsync("api/mobile/app-data", request);

        response.EnsureSuccessStatusCode();

        _cachedValues = null;
    }

    private int _cachedLine;
    private async Task<List<BasicMachineValuesDto>> GetLatestValuesAsync()
    {
        

        if (_cachedValues is not null &&
            _cachedLine == SelectedLine &&
            DateTime.UtcNow.Subtract(_lastLoadedUtc).TotalSeconds < 5)
        {
            return _cachedValues;
        }

        var values = await _httpClient.GetFromJsonAsync<List<BasicMachineValuesDto>>(
            $"api/mobile/latest-basic-values?line={SelectedLine}");

        _cachedValues = values ?? new List<BasicMachineValuesDto>();
        _lastLoadedUtc = DateTime.UtcNow;

        _cachedLine = SelectedLine;

        return _cachedValues;
    }

    private static bool IsDosingMachine(BasicMachineValuesDto item)
    {
        return item.MachineType.Equals("Dosing", StringComparison.OrdinalIgnoreCase)
            || item.MachineName.StartsWith("DosingMachine", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDrenchDosingMachine(BasicMachineValuesDto item)
    {
        return item.MachineType.Equals("Drench Dosing", StringComparison.OrdinalIgnoreCase)
            || item.MachineName.StartsWith("DrenchDosingMachine", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWaxMachine(BasicMachineValuesDto item)
    {
        return item.MachineType.Equals("Wax", StringComparison.OrdinalIgnoreCase)
            || item.MachineName.StartsWith("WaxMachine", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAppData(BasicMachineValuesDto item)
    {
        return item.MachineType.Equals("App Data", StringComparison.OrdinalIgnoreCase)
            || item.MachineName.StartsWith("Line", StringComparison.OrdinalIgnoreCase);
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

    private static string NormalizeAppDataField(string parameter)
    {
        return parameter switch
        {
            "Titration" => "Titration",
            "Temperature" => "Temperature",
            "pH" => "pH",
            "Wax" => "Wax",
            "DrenchDose" => "DrenchDose",
            "Drench Dose" => "DrenchDose",
            _ => parameter
        };
    }

    public async Task<List<PhHistoryPointDto>> GetPhHistoryAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PhHistoryPointDto>>(
            $"api/mobile/ph-history?line={SelectedLine}&take=30");

        return result ?? new List<PhHistoryPointDto>();
    }

    public async Task<List<MetricHistoryPointDto>> GetMetricHistoryAsync(string metric)
    {
        var result = await _httpClient.GetFromJsonAsync<List<MetricHistoryPointDto>>(
            $"api/mobile/metric-history?line={SelectedLine}&metric={Uri.EscapeDataString(metric)}&take=30");

        return result ?? new List<MetricHistoryPointDto>();
    }

    public async Task<List<AlarmMessageDto>> GetAlarmsAsync(int line, string machine)
    {
        var result = await _httpClient.GetFromJsonAsync<List<AlarmMessageDto>>(
            $"api/mobile/alarms?line={line}&machine={Uri.EscapeDataString(machine)}&take=10");

        return result ?? new List<AlarmMessageDto>();
    }
}