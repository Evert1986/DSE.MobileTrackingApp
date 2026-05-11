namespace DSE.MobileTracking.Api.Models;

public sealed class TitrationInputDto
{
    public int Line { get; set; } = 1;
    public decimal Titration { get; set; }
}