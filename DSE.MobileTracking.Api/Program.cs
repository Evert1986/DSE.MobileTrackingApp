using Dapper;
using DSE.MobileTracking.Api.Data;
using DSE.MobileTracking.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI support
builder.Services.AddOpenApi();

// Dapper SQL connection factory
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IBasicValuesRepository, BasicValuesRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    Application = "DSE Mobile Tracking API",
    Status = "Running",
    TimeUtc = DateTime.UtcNow
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "Healthy",
    TimeUtc = DateTime.UtcNow
}));

app.MapGet("/api/health/database", async (ISqlConnectionFactory connectionFactory) =>
{
    try
    {
        using var connection = connectionFactory.CreateConnection();

        var result = await connection.ExecuteScalarAsync<int>("SELECT 1");

        return Results.Ok(new
        {
            Status = "Database connection OK",
            Result = result,
            TimeUtc = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Database connection failed",
            detail: ex.Message,
            statusCode: 500);
    }
});

app.MapGet("/api/mobile/latest-basic-values", async (int? line, IBasicValuesRepository repository) =>
{
    var result = await repository.GetLatestBasicValuesAsync(line ?? 1);
    return Results.Ok(result);
});

app.MapGet("/api/mobile/current-run", async (int? line, IBasicValuesRepository repository) =>
{
    var result = await repository.GetCurrentRunAsync(line ?? 1);
    return Results.Ok(result);
});

app.MapPost("/api/mobile/app-data", async (
    AppDataInputDto input,
    IBasicValuesRepository repository) =>
{
    if (input.Line != 1 && input.Line != 2)
    {
        return Results.BadRequest(new
        {
            Error = "Line must be 1 or 2."
        });
    }

    if (string.IsNullOrWhiteSpace(input.Field))
    {
        return Results.BadRequest(new
        {
            Error = "Field is required."
        });
    }

    await repository.SaveAppDataAsync(input.Line, input.Field, input.Value);

    return Results.Ok(new
    {
        Status = "App data saved",
        Line = input.Line,
        Field = input.Field,
        Value = input.Value,
        TimeUtc = DateTime.UtcNow
    });
});

app.MapGet("/api/mobile/ph-history", async (
    int? line,
    int? take,
    IBasicValuesRepository repository) =>
{
    var result = await repository.GetPhHistoryAsync(line ?? 1, take ?? 30);
    return Results.Ok(result);
});

app.MapGet("/api/mobile/metric-history", async (
    int? line,
    string? metric,
    int? take,
    IBasicValuesRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(metric))
    {
        return Results.BadRequest(new
        {
            Error = "Metric is required."
        });
    }

    try
    {
        var result = await repository.GetMetricHistoryAsync(line ?? 1, metric, take ?? 30);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new
        {
            Error = ex.Message
        });
    }
});

app.MapGet("/api/mobile/alarms", async (
    int? line,
    string? machine,
    int? take,
    IBasicValuesRepository repository) =>
    {
    var result = await repository.GetAlarmsAsync(
        line ?? 1,
        machine ?? "Wax",
        take ?? 10);

    return Results.Ok(result);
});

app.Run();