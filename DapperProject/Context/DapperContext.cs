using Microsoft.Data.SqlClient;

namespace DapperProject.Context;

public sealed class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TradePulse")
            ?? throw new InvalidOperationException("TradePulse bağlantı dizesi bulunamadı.");
    }

    public SqlConnection CreateConnection() => new(_connectionString);

    public SqlConnection CreateMasterConnection()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "master" };
        return new SqlConnection(builder.ConnectionString);
    }

    public string DatabaseName => new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
}
