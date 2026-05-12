using DSE.MobileTrackingApp.Models;

namespace DSE.MobileTrackingApp.Services;

public class MockTrackingDataService : ITrackingDataService
{
    public int SelectedLine { get; set; } = 1;
    public Task<CurrentRun> GetCurrentRunAsync()
    {
        return Task.FromResult(new CurrentRun(
            Facility: "Demo Facility",
            Packline: "Packline 1",
            Variety: "Valencia Late",
            BatchId: "VL-240515-01",
            OperatorName: "David M.",
            StartTime: DateTime.Today.AddHours(7).AddMinutes(30),
            IsRunning: true
        ));
    }

    public Task<List<ParameterReading>> GetParameterReadingsAsync()
    {
        return Task.FromResult(new List<ParameterReading>
        {
            new("pH", "💧", 5.5m, "", 5.0m, 7.0m, true),
            new("Temperature", "🌡️", 21.4m, "°C", 18.0m, 24.0m, true),
            new("Titration", "🧪", 565m, "ppm", 550m, 600m, true),
            new("Wax", "🛢️", 1.1m, "L/Ton", 0.8m, 1.5m, true),
            new("Drench Dose1", "💧", 1.10m, "L/ton", 0.8m, 1.5m, true)
        });
    }

    public Task<List<HistoryReading>> GetHistoryAsync()
    {
        return Task.FromResult(new List<HistoryReading>
        {
            new("pH", "💧", 5.5m, "", DateTime.Today.AddHours(8).AddMinutes(45), true),
            new("Temperature", "🌡️", 21.4m, "°C", DateTime.Today.AddHours(8).AddMinutes(45), true),
            new("Titration", "🧪", 565m, "ppm", DateTime.Today.AddHours(8).AddMinutes(45), true),
            new("Wax", "🛢️", 1.1m, "L/Ton", DateTime.Today.AddHours(8).AddMinutes(45), true),
            new("pH", "💧", 5.4m, "", DateTime.Today.AddHours(7).AddMinutes(45), true)
        });
    }

    public Task<List<TrackingAlert>> GetAlertsAsync()
    {
        return Task.FromResult(new List<TrackingAlert>
        {
            new("No Titration Reading", "No titration reading recorded in the last 2 hours.", "warning", DateTime.Today.AddHours(8).AddMinutes(10))
        });
    }

    public Task<List<PhHistoryPointDto>> GetPhHistoryAsync()
    {
        var now = DateTime.Now;

        var values = Enumerable.Range(0, 30)
            .Select(i => new PhHistoryPointDto
            {
                DateOfLog = now.AddMinutes(-30 * (29 - i)),
                PH = 5.5m + ((i % 5) * 0.1m),
                IsInRange = true
            })
            .ToList();

        return Task.FromResult(values);
    }

    public Task<List<MetricHistoryPointDto>> GetMetricHistoryAsync(string metric)
    {
        var now = DateTime.Now;

        var values = Enumerable.Range(0, 30)
            .Select(i => new MetricHistoryPointDto
            {
                DateOfLog = now.AddMinutes(-5 * (29 - i)),
                Value = metric switch
                {
                    "Temperature" => 22m + ((i % 5) * 0.2m),
                    "Tons" => 18m + ((i % 5) * 0.5m),
                    "Wax" => 1.1m + ((i % 5) * 0.02m),
                    _ => 5.8m + ((i % 5) * 0.05m)
                },
                IsInRange = true
            })
            .ToList();

        return Task.FromResult(values);
    }

    public Task SaveReadingAsync(ReadingInput input)
    {
        return Task.CompletedTask;
    }
}
