namespace DSE.MobileTracking.Api.Models;

public sealed class CurrentRunDto
{
    public string Facility { get; set; } = "BBI";
    public string Packline { get; set; } = "Packline 1";
    public string Variety { get; set; } = "";
    public string BatchId { get; set; } = "";
    public string OperatorName { get; set; } = "David M";
    public DateTime StartTime { get; set; }
    public string ActualStatus { get; set; } = "";
    public bool IsRunning { get; set; }
}