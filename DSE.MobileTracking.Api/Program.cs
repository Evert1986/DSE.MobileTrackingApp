using Dapper;
using DSE.MobileTracking.Api.Data;

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

app.Run();