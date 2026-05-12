using System.Data;
using Dapper;
using DSE.MobileTracking.Api.Models;



namespace DSE.MobileTracking.Api.Data;

public interface IBasicValuesRepository
{
    Task<IReadOnlyList<BasicMachineValuesDto>> GetLatestBasicValuesAsync(int line);
    Task<CurrentRunDto> GetCurrentRunAsync(int line);
    Task SaveAppDataAsync(int line, string field, decimal value);
    Task<IReadOnlyList<PhHistoryPointDto>> GetPhHistoryAsync(int line, int take);
    Task<IReadOnlyList<MetricHistoryPointDto>> GetMetricHistoryAsync(int line, string metric, int take);
}

public sealed class BasicValuesRepository : IBasicValuesRepository
{

    private readonly ISqlConnectionFactory _connectionFactory;

    public BasicValuesRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<BasicMachineValuesDto>> GetLatestBasicValuesAsync(int line)
    {
        line = NormalizeLine(line);

        using var connection = _connectionFactory.CreateConnection();

        var results = new List<BasicMachineValuesDto>
    {
        await GetLatestFromTableAsync(connection, $"DosingMachine{line}", line, "Dosing"),
        await GetLatestFromTableAsync(connection, $"DrenchDosingMachine{line}", line, "Drench Dosing"),
        await GetLatestWaxFromTableAsync(connection, $"WaxMachine{line}", line, "Wax"),
        await GetLatestTitrationFromTableAsync(connection, $"Line{line}AppData", line, "App Data")
    };

        return results;
    }

    public async Task<CurrentRunDto> GetCurrentRunAsync(int line)
    {
        line = NormalizeLine(line);

        var tableName = $"SizerOnlyDataLine{line}";

        var sql = $"""
    SELECT TOP 1
        [CurrentBatchId],
        [CurrentBatchVarietyName],
        [CurrentBatchStartTime],
        [ActualStatus]
    FROM [dbo].[{tableName}]
    ORDER BY [DateOfLog] DESC;
    """;

        using var connection = _connectionFactory.CreateConnection();

        var row = await connection.QuerySingleOrDefaultAsync<SizerCurrentRunRow>(sql);

        if (row is null)
        {
            return new CurrentRunDto
            {
                Facility = "BBI",
                Packline = $"Packline {line}",
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
            Packline = $"Packline {line}",
            Variety = row.CurrentBatchVarietyName ?? "",
            BatchId = row.CurrentBatchId ?? "",
            OperatorName = "David M",
            StartTime = row.CurrentBatchStartTime ?? DateTime.Now,
            ActualStatus = string.IsNullOrWhiteSpace(actualStatus)
                ? "STOPPED"
                : actualStatus.ToUpperInvariant(),
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

    private static async Task<BasicMachineValuesDto> GetLatestWaxFromTableAsync(
    System.Data.IDbConnection connection,
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
            [litresPerTon] AS LitresPerTon
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

    private static async Task<BasicMachineValuesDto> GetLatestTitrationFromTableAsync(
    System.Data.IDbConnection connection,
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
            [Value] AS Titration
        FROM [dbo].[{tableName}]
        WHERE [Field] = 'Titration'
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

    public async Task SaveAppDataAsync(int line, string field, decimal value)
    {
        line = NormalizeLine(line);

        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Titration",
            "Temperature",
            "pH",
            "Wax",
            "DrenchDose"
        };

        if (!allowedFields.Contains(field))
        {
            throw new InvalidOperationException($"Invalid app data field: {field}");
        }

        var tableName = $"Line{line}AppData";

        var sql = $"""
        INSERT INTO [dbo].[{tableName}]
        (
            [Field],
            [Value],
            [DateOfLog]
        )
        VALUES
        (
            @Field,
            @Value,
            CAST(SYSUTCDATETIME() AT TIME ZONE 'UTC' AT TIME ZONE 'South Africa Standard Time' AS datetime)
        );
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new
        {
            Field = field,
            Value = value
        });
    }

    public async Task<IReadOnlyList<PhHistoryPointDto>> GetPhHistoryAsync(int line, int take)
    {
        line = NormalizeLine(line);
        take = Math.Clamp(take, 1, 100);

        var tableName = $"DosingMachine{line}";

        var sql = $"""
            SELECT TOP (@Take)
                [DateOfLog],
                [PH]
            FROM [dbo].[{tableName}]
            WHERE [PH] IS NOT NULL
            ORDER BY [DateOfLog] DESC;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<PhHistoryPointDto>(sql, new
        {
            Take = take
        });

        return rows
            .Select(x =>
            {
                x.IsInRange = x.PH >= 5.0m && x.PH <= 7.0m;
                return x;
            })
            .OrderBy(x => x.DateOfLog)
            .ToList();
    }

    public async Task<IReadOnlyList<MetricHistoryPointDto>> GetMetricHistoryAsync(int line, string metric, int take)
    {
        line = NormalizeLine(line);
        take = Math.Clamp(take, 1, 100);

        var metricKey = metric.Trim().ToLowerInvariant();

        string tableName;
        string columnName;
        decimal targetMin;
        decimal targetMax;

        switch (metricKey)
        {
            case "ph":
                tableName = $"DosingMachine{line}";
                columnName = "PH";
                targetMin = 5.0m;
                targetMax = 7.0m;
                break;

            case "temperature":
                tableName = $"DosingMachine{line}";
                columnName = "Temperature";
                targetMin = 18.0m;
                targetMax = 24.0m;
                break;

            case "tons":
                tableName = $"DosingMachine{line}";
                columnName = "TonsPerHour";
                targetMin = 0m;
                targetMax = 100m;
                break;

            case "wax":
                tableName = $"WaxMachine{line}";
                columnName = "litresPerTon";
                targetMin = 0.8m;
                targetMax = 1.5m;
                break;

            default:
                throw new InvalidOperationException($"Unsupported metric: {metric}");
        }

        var sql = $"""
        SELECT TOP (@Take)
            [DateOfLog],
            [{columnName}] AS [Value]
        FROM [dbo].[{tableName}]
        WHERE [{columnName}] IS NOT NULL
        ORDER BY [DateOfLog] DESC;
        """;

        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<MetricHistoryPointDto>(sql, new
        {
            Take = take
        });

        return rows
            .Select(x =>
            {
                x.IsInRange = x.Value >= targetMin && x.Value <= targetMax;
                return x;
            })
            .OrderBy(x => x.DateOfLog)
            .ToList();
    }

    private static int NormalizeLine(int line)
    {
        return line == 2 ? 2 : 1;
    }
}