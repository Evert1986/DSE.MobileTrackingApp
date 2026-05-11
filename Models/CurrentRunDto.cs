namespace DSE.MobileTrackingApp.Models;

public sealed class CurrentRunDto
{
    public string Facility { get; set; } = "";
    public string Packline { get; set; } = "";
    public string Variety { get; set; } = "";
    public string BatchId { get; set; } = "";
    public string OperatorName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public string ActualStatus { get; set; } = "";
    public bool IsRunning { get; set; }
}