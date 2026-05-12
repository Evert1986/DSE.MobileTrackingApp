namespace DSE.MobileTrackingApp.Models;

public sealed class PhHistoryPointDto
{
    public DateTime DateOfLog { get; set; }
    public decimal PH { get; set; }
    public bool IsInRange { get; set; }
}
