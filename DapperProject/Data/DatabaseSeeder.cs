using System.Data;
using Dapper;
using DapperProject.Context;
using Microsoft.Data.SqlClient;

namespace DapperProject.Data;

public sealed class DatabaseSeeder(
    DapperContext context,
    IConfiguration configuration,
    ILogger<DatabaseSeeder> logger)
{
    private const string SeedKey = "TradeLogs_1M_v1";
    private readonly int _recordCount = configuration.GetValue("TradePulse:SeedRecordCount", 1_000_000);
    private readonly int _batchSize = configuration.GetValue("TradePulse:BulkBatchSize", 50_000);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);
        await using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(SchemaSql, commandTimeout: 120, cancellationToken: cancellationToken));

        var seeded = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM SeedHistory WHERE SeedKey = @SeedKey", new { SeedKey }, cancellationToken: cancellationToken));

        if (seeded == 0)
        {
            logger.LogInformation("TradePulse veri seti hazırlanıyor: {Count:N0} kayıt.", _recordCount);
            await connection.ExecuteAsync(new CommandDefinition("TRUNCATE TABLE TradeLogs", cancellationToken: cancellationToken));
            await BulkInsertAsync(connection, cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO SeedHistory (SeedKey, RecordCount, CompletedAt) VALUES (@SeedKey, @RecordCount, SYSUTCDATETIME())",
                new { SeedKey, RecordCount = _recordCount }, cancellationToken: cancellationToken));
        }

        await connection.ExecuteAsync(new CommandDefinition(IndexSql, commandTimeout: 300, cancellationToken: cancellationToken));
        logger.LogInformation("TradePulse veritabanı hazır.");
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var master = context.CreateMasterConnection();
        await master.OpenAsync(cancellationToken);
        var safeName = context.DatabaseName.Replace("]", "]]", StringComparison.Ordinal);
        var sql = $"IF DB_ID(@DatabaseName) IS NULL CREATE DATABASE [{safeName}]";
        await master.ExecuteAsync(new CommandDefinition(sql, new { DatabaseName = context.DatabaseName }, commandTimeout: 120, cancellationToken: cancellationToken));
    }

    private async Task BulkInsertAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        var random = new Random(20260707);
        var pairs = new[]
        {
            (Name: "BTC/USDT", Base: 68_500m, Spread: 12_000m, MaxQty: 2.5m),
            (Name: "ETH/USDT", Base: 3_650m, Spread: 900m, MaxQty: 30m),
            (Name: "BNB/USDT", Base: 610m, Spread: 130m, MaxQty: 120m),
            (Name: "SOL/USDT", Base: 155m, Spread: 65m, MaxQty: 500m),
            (Name: "XRP/USDT", Base: 0.58m, Spread: 0.24m, MaxQty: 80_000m),
            (Name: "ADA/USDT", Base: 0.44m, Spread: 0.20m, MaxQty: 90_000m),
            (Name: "AVAX/USDT", Base: 38m, Spread: 16m, MaxQty: 2_000m),
            (Name: "DOGE/USDT", Base: 0.14m, Spread: 0.07m, MaxQty: 250_000m)
        };
        var countries = new[] { "Türkiye", "ABD", "Almanya", "Birleşik Krallık", "Japonya", "Güney Kore", "Singapur", "Brezilya", "Kanada", "Fransa", "Hindistan", "BAE" };
        var end = DateTime.UtcNow;
        var start = end.Date.AddDays(-364);
        var dateRangeSeconds = (long)(end - start).TotalSeconds;

        for (var offset = 0; offset < _recordCount; offset += _batchSize)
        {
            var size = Math.Min(_batchSize, _recordCount - offset);
            using var table = CreateTradeTable();
            for (var i = 0; i < size; i++)
            {
                var id = offset + i + 1;
                var pair = pairs[random.Next(pairs.Length)];
                var priceFactor = ((decimal)random.NextDouble() - 0.5m) * pair.Spread;
                var price = Math.Max(0.0001m, Math.Round(pair.Base + priceFactor, 8));
                var quantity = Math.Round(0.001m + ((decimal)random.NextDouble() * pair.MaxQty), 8);
                var total = Math.Round(price * quantity, 4);
                var tradeType = random.NextDouble() < 0.515 ? "BUY" : "SELL";
                var feeRate = id % 20 == 0 ? 0.00075m : 0.001m;
                var date = start.AddSeconds(random.NextInt64(dateRangeSeconds + 1));

                table.Rows.Add(id, $"USR-{random.Next(1, 85_001):D6}", pair.Name, tradeType, price, quantity,
                    total, Math.Round(total * feeRate, 4), countries[random.Next(countries.Length)], random.Next(8, 241), date);
            }

            using var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, null)
            {
                DestinationTableName = "dbo.TradeLogs",
                BatchSize = size,
                BulkCopyTimeout = 300,
                EnableStreaming = true
            };
            foreach (DataColumn column in table.Columns)
                bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            await bulk.WriteToServerAsync(table, cancellationToken);
            logger.LogInformation("Seed ilerlemesi: {Inserted:N0}/{Total:N0}", offset + size, _recordCount);
        }
    }

    private static DataTable CreateTradeTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("UserCode", typeof(string));
        table.Columns.Add("CryptoPair", typeof(string));
        table.Columns.Add("TradeType", typeof(string));
        table.Columns.Add("Price", typeof(decimal));
        table.Columns.Add("Quantity", typeof(decimal));
        table.Columns.Add("TotalUSD", typeof(decimal));
        table.Columns.Add("FeeUSD", typeof(decimal));
        table.Columns.Add("LocationCountry", typeof(string));
        table.Columns.Add("ExecutionTimeMs", typeof(int));
        table.Columns.Add("TransactionDate", typeof(DateTime));
        return table;
    }

    private const string SchemaSql = """
        IF OBJECT_ID(N'dbo.TradeLogs', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.TradeLogs
            (
                Id INT NOT NULL CONSTRAINT PK_TradeLogs PRIMARY KEY CLUSTERED,
                UserCode NVARCHAR(20) NOT NULL,
                CryptoPair NVARCHAR(20) NOT NULL,
                TradeType NVARCHAR(4) NOT NULL,
                Price DECIMAL(20,8) NOT NULL,
                Quantity DECIMAL(20,8) NOT NULL,
                TotalUSD DECIMAL(20,4) NOT NULL,
                FeeUSD DECIMAL(20,4) NOT NULL,
                LocationCountry NVARCHAR(60) NOT NULL,
                ExecutionTimeMs INT NOT NULL,
                TransactionDate DATETIME NOT NULL,
                CONSTRAINT CK_TradeLogs_TradeType CHECK (TradeType IN (N'BUY', N'SELL'))
            );
        END;

        IF OBJECT_ID(N'dbo.SeedHistory', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.SeedHistory
            (
                SeedKey NVARCHAR(100) NOT NULL CONSTRAINT PK_SeedHistory PRIMARY KEY,
                RecordCount INT NOT NULL,
                CompletedAt DATETIME2 NOT NULL
            );
        END;
        """;

    private const string IndexSql = """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_TransactionDate' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
            CREATE NONCLUSTERED INDEX IX_TradeLogs_TransactionDate ON dbo.TradeLogs (TransactionDate DESC, Id DESC)
            INCLUDE (CryptoPair, TradeType, TotalUSD, FeeUSD, ExecutionTimeMs);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_CryptoPair' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
            CREATE NONCLUSTERED INDEX IX_TradeLogs_CryptoPair ON dbo.TradeLogs (CryptoPair)
            INCLUDE (TradeType, TotalUSD, TransactionDate);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_TradeType' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
            CREATE NONCLUSTERED INDEX IX_TradeLogs_TradeType ON dbo.TradeLogs (TradeType)
            INCLUDE (TotalUSD);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_TotalUSD' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
            CREATE NONCLUSTERED INDEX IX_TradeLogs_TotalUSD ON dbo.TradeLogs (TotalUSD DESC, Id DESC)
            INCLUDE (UserCode, CryptoPair, TradeType, Price, Quantity, FeeUSD, LocationCountry, ExecutionTimeMs, TransactionDate);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TradeLogs_LocationCountry' AND object_id = OBJECT_ID(N'dbo.TradeLogs'))
            CREATE NONCLUSTERED INDEX IX_TradeLogs_LocationCountry ON dbo.TradeLogs (LocationCountry)
            INCLUDE (TotalUSD);
        """;
}
