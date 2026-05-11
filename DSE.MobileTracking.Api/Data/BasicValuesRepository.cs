using System.Data;
using Dapper;
using DSE.MobileTracking.Api.Models;



namespace DSE.MobileTracking.Api.Data;

public interface IBasicValuesRepository
{
    Task<IReadOnlyList<BasicMachineValuesDto>> GetLatestBasicValuesAsync();
    Task<CurrentRunDto> GetCurrentRunAsync();
}

public sealed class BasicValuesRepository : IBasicValuesRepository
{

    private readonly ISqlConnectionFactory _connectionFactory;

    public BasicValuesRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<BasicMachineValuesDto>> GetLatestBasicValuesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var results = new List<BasicMachineValuesDto>
        {
            await GetLatestFromTableAsync(connection, "DosingMachine1", 1, "Dosing"),
            await GetLatestFromTableAsync(connection, "DosingMachine2", 2, "Dosing"),
            await GetLatestFromTableAsync(connection, "DrenchDosingMachine1", 1, "Drench Dosing"),
            await GetLatestFromTableAsync(connection, "DrenchDosingMachine2", 2, "Drench Dosing")
        };

        return results;
    }

    public async Task<CurrentRunDto> GetCurrentRunAsync()
    {
        const string sql = """
            SELECT TOP 1
                [CurrentBatchId],
                [CurrentBatchVarietyName],
                [CurrentBatchStartTime],
                [ActualStatus]
            FROM [dbo].[SizerOnlyDataLine1]
            ORDER BY [DateOfLog] DESC;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<SizerCurrentRunRow>(sql);

        if (row is null)
        {
            return new CurrentRunDto
            {
                Facility = "BBI",
                Packline = "Packline 1",
                Variety = "No Data",
                BatchId = "-",
                OperatorName = "David M",
                StartTime = DateTime.Now,
                ActualStatus = "STOPPED",
                IsRunning = false
            };
        }

        var actualStatus = row.ActualStatus?.Trim() ?? "";
        var isRunning = actualStatus.Equals("RUNNING", StringComparison.OrdinalIgnoreCase);

        return new CurrentRunDto
        {
            Facility = "BBI",
            Packline = "Packline 1",
            Variety = row.CurrentBatchVarietyName ?? "",
            BatchId = row.CurrentBatchId ?? "",
            OperatorName = "David M",
            StartTime = row.CurrentBatchStartTime ?? DateTime.Now,
            ActualStatus = string.IsNullOrWhiteSpace(actualStatus) ? "STOPPED" : actualStatus.ToUpperInvariant(),
            IsRunning = isRunning
        };
    }

    private sealed class SizerCurrentRunRow
    {
        public string? CurrentBatchId { get; set; }
        public string? CurrentBatchVarietyName { get; set; }
        public DateTime? CurrentBatchStartTime { get; set; }
        public string? ActualStatus { get; set; }
    }

    private static async Task<BasicMachineValuesDto> GetLatestFromTableAsync(
        IDbConnection connection,
        string tableName,
        int line,
        string machineType)
    {
        var sql = $"""
        SELECT TOP 1
            '{tableName}' AS MachineName,
            @Line AS Line,
            @MachineType AS MachineType,
            [DateOfLog],
            [PH],
            [Temperature],
            [TonsPerHour],
            [BinCountSinceLastReset]
        FROM [dbo].[{tableName}]
        ORDER BY [DateOfLog] DESC;
        """;

        var result = await connection.QuerySingleOrDefaultAsync<BasicMachineValuesDto>(
            sql,
            new
            {
                Line = line,
                MachineType = machineType
            });

        return result ?? new BasicMachineValuesDto
        {
            MachineName = tableName,
            Line = line,
            MachineType = machineType
        };
    }
}