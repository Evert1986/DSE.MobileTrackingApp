namespace DSE.MobileTrackingApp.Models;

public sealed class AppDataInputDto
{
    public int Line { get; set; }
    public string Field { get; set; } = "";
    public decimal Value { get; set; }
}