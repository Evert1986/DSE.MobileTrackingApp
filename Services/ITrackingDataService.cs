using DSE.MobileTrackingApp.Models;

namespace DSE.MobileTrackingApp.Services;

public interface ITrackingDataService
{
    Task<CurrentRun> GetCurrentRunAsync();
    Task<List<ParameterReading>> GetParameterReadingsAsync();
    Task<List<HistoryReading>> GetHistoryAsync();
    Task<List<TrackingAlert>> GetAlertsAsync();
    Task SaveReadingAsync(ReadingInput input);
}
