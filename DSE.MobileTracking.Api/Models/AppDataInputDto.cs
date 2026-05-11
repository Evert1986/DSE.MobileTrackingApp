namespace DSE.MobileTracking.Api.Models;

public sealed class AppDataInputDto
{
    public int Line { get; set; } = 1;
    public string Field { get; set; } = "";
    public decimal Value { get; set; }
}