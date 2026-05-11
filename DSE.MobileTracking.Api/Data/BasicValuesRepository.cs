using System.Data;
using Dapper;
using DSE.MobileTracking.Api.Models;

namespace DSE.MobileTracking.Api.Data;

public interface IBasicValuesRepository
{
    Task<IReadOnlyList<BasicMachineValuesDto>> GetLatestBasicValuesAsync();
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