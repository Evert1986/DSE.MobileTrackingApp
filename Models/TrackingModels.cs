namespace DSE.MobileTrackingApp.Models;

public record CurrentRun(
    string Facility,
    string Packline,
    string Variety,
    string BatchId,
    string OperatorName,
    DateTime StartTime,
    bool IsRunning
);

public record ParameterReading(
    string Name,
    string Icon,
    decimal Value,
    string Unit,
    decimal TargetMin,
    decimal TargetMax,
    bool IsInRange
);

public record HistoryReading(
    string Parameter,
    string Icon,
    decimal Value,
    string Unit,
    DateTime ReadingTime,
    bool IsInRange
);

public record TrackingAlert(
    string Title,
    string Message,
    string Severity,
    DateTime Time
);

public record ReadingInput(
    string Parameter,
    string Unit,
    decimal? Value,
    string Notes
);
