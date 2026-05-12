using DSE.MobileTrackingApp.Models;

namespace DSE.MobileTrackingApp.Services;

public interface ITrackingDataService
{
    int SelectedLine { get; set; }
    Task<CurrentRun> GetCurrentRunAsync();
    Task<List<ParameterReading>> GetParameterReadingsAsync();
    Task<List<HistoryReading>> GetHistoryAsync();
    Task<List<TrackingAlert>> GetAlertsAsync();
    Task SaveReadingAsync(ReadingInput input);
    Task<List<PhHistoryPointDto>> GetPhHistoryAsync();
    Task<List<MetricHistoryPointDto>> GetMetricHistoryAsync(string metric);
    Task<List<AlarmMessageDto>> GetAlarmsAsync(int line, string machine);

}
