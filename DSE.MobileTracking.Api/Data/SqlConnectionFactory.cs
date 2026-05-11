using System.Data;
using Microsoft.Data.SqlClient;

namespace DSE.MobileTracking.Api.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TrackingDb")
            ?? throw new InvalidOperationException("Connection string 'TrackingDb' was not found.");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}