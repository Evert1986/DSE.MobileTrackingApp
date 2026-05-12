namespace DSE.MobileTracking.Api.Models;

public sealed class AlarmMessageDto
{
    public DateTime DateOfEvent { get; set; }
    public string MachineName { get; set; } = "";
    public string MachineError { get; set; } = "";
    public string ErrorCategory { get; set; } = "";
    public string NotificationType { get; set; } = "";
    public string ErrStatus { get; set; } = "";
    public string VisualState { get; set; } = "";
}