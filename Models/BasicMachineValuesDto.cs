namespace DSE.MobileTrackingApp.Models;

public sealed class BasicMachineValuesDto
{
    public string MachineName { get; set; } = "";
    public int Line { get; set; }
    public string MachineType { get; set; } = "";
    public DateTime? DateOfLog { get; set; }
    public decimal? PH { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? TonsPerHour { get; set; }
    public decimal? BinCountSinceLastReset { get; set; }

    public decimal? LitresPerTon { get; set; }
    public decimal? Titration { get; set; }
}