namespace DSE.MobileTracking.Api.Models;

public sealed class MetricHistoryPointDto
{
    public DateTime DateOfLog { get; set; }
    public decimal Value { get; set; }
    public bool IsInRange { get; set; }
}