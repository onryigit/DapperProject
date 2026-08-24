using Dapper;
using DapperProject.Context;
using DapperProject.Models;

namespace DapperProject.Services;

public sealed class TradeRepository(DapperContext context) : ITradeRepository
{
    private const string Columns = "Id, UserCode, CryptoPair, TradeType, Price, Quantity, TotalUSD, FeeUSD, LocationCountry, ExecutionTimeMs, TransactionDate";

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            DECLARE @AsOf DATETIME = (SELECT MAX(TransactionDate) FROM TradeLogs);

            SELECT
                COALESCE(SUM(TotalUSD), 0) AS TotalVolume,
                COALESCE(SUM(FeeUSD), 0) AS TotalFees,
                COALESCE(AVG(CAST(ExecutionTimeMs AS DECIMAL(18,2))), 0) AS AverageExecutionTime,
                COUNT(*) AS TotalTrades,
                MAX(TransactionDate) AS LatestTransactionDate,
                COALESCE(SUM(CASE WHEN TransactionDate > DATEADD(DAY, -30, @AsOf) AND TransactionDate <= @AsOf THEN TotalUSD ELSE 0 END), 0) AS CurrentPeriodVolume,
                COALESCE(SUM(CASE WHEN TransactionDate > DATEADD(DAY, -60, @AsOf) AND TransactionDate <= DATEADD(DAY, -30, @AsOf) THEN TotalUSD ELSE 0 END), 0) AS PreviousPeriodVolume,
                COALESCE(SUM(CASE WHEN TransactionDate > DATEADD(DAY, -30, @AsOf) AND TransactionDate <= @AsOf THEN FeeUSD ELSE 0 END), 0) AS CurrentPeriodFees,
                COALESCE(SUM(CASE WHEN TransactionDate > DATEADD(DAY, -60, @AsOf) AND TransactionDate <= DATEADD(DAY, -30, @AsOf) THEN FeeUSD ELSE 0 END), 0) AS PreviousPeriodFees,
                COALESCE((SELECT TOP (1) CryptoPair FROM TradeLogs GROUP BY CryptoPair ORDER BY SUM(TotalUSD) DESC), N'—') AS HighestVolumePair
            FROM TradeLogs;

            SELECT CAST(TransactionDate AS DATE) AS TradeDate, SUM(TotalUSD) AS Volume
            FROM TradeLogs
            WHERE TransactionDate >= DATEADD(DAY, -29, CAST(@AsOf AS DATE))
              AND TransactionDate <= @AsOf
            GROUP BY CAST(TransactionDate AS DATE)
            ORDER BY TradeDate;

            SELECT CryptoPair, COUNT_BIG(*) AS TradeCount, SUM(TotalUSD) AS Volume
            FROM TradeLogs
            GROUP BY CryptoPair
            ORDER BY Volume DESC;

            SELECT TradeType, COUNT_BIG(*) AS TradeCount
            FROM TradeLogs
            GROUP BY TradeType
            ORDER BY TradeType;

            SELECT TOP (5) Id, UserCode, CryptoPair, TradeType, Price, Quantity, TotalUSD, FeeUSD, LocationCountry, ExecutionTimeMs, TransactionDate
            FROM TradeLogs
            ORDER BY TotalUSD DESC, Id DESC;

            SELECT TOP (10) LocationCountry, COUNT_BIG(*) AS TradeCount, SUM(TotalUSD) AS Volume
            FROM TradeLogs
            GROUP BY LocationCountry
            ORDER BY TradeCount DESC;
            """;

        await using var connection = context.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, commandTimeout: 120, cancellationToken: cancellationToken));

        var summary = await multi.ReadSingleAsync<DashboardSummary>();
        var trend = (await multi.ReadAsync<VolumePoint>()).AsList();
        var pairs = (await multi.ReadAsync<PairDistribution>()).AsList();
        var types = (await multi.ReadAsync<TradeTypeDistribution>()).AsList();
        var top = (await multi.ReadAsync<TradeLog>()).AsList();
        var countries = (await multi.ReadAsync<CountryActivity>()).AsList();

        ApplyCoordinates(countries);
        var buyCount = types.FirstOrDefault(x => x.TradeType == "BUY")?.TradeCount ?? 0;
        var totalTypeCount = types.Sum(x => x.TradeCount);

        return new DashboardViewModel
        {
            Summary = summary,
            VolumeTrend = trend,
            PairDistribution = pairs,
            TradeTypeDistribution = types,
            TopTransactions = top,
            CountryActivity = countries,
            BuyPressure = totalTypeCount == 0 ? 0 : Math.Round(buyCount * 100m / totalTypeCount, 1),
            VolumeChangePercentage = CalculatePercentageChange(summary.CurrentPeriodVolume, summary.PreviousPeriodVolume),
            FeeChangePercentage = CalculatePercentageChange(summary.CurrentPeriodFees, summary.PreviousPeriodFees),
            DatasetTargetUsage = Math.Min(100m, Math.Round(summary.TotalTrades * 100m / 1_000_000m, 1))
        };
    }

    public async Task<PagedResult<TradeLog>> GetPagedAsync(int page, int pageSize, int? id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DECLARE @TotalCount INT = (SELECT COUNT(1) FROM TradeLogs WHERE (@Id IS NULL OR Id = @Id));
            DECLARE @TotalPages INT = CASE WHEN @TotalCount = 0 THEN 1 ELSE CEILING(@TotalCount * 1.0 / @PageSize) END;
            DECLARE @EffectivePage INT = CASE WHEN @Page > @TotalPages THEN @TotalPages ELSE @Page END;

            SELECT @TotalCount AS TotalCount, @EffectivePage AS Page;
            SELECT Id, UserCode, CryptoPair, TradeType, Price, Quantity, TotalUSD, FeeUSD, LocationCountry, ExecutionTimeMs, TransactionDate
            FROM TradeLogs
            WHERE (@Id IS NULL OR Id = @Id)
            ORDER BY TransactionDate DESC, Id DESC
            OFFSET ((@EffectivePage - 1) * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new { Id = id, Page = page, PageSize = pageSize };
        await using var connection = context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        var metadata = await multi.ReadSingleAsync<PageMetadata>();
        var items = (await multi.ReadAsync<TradeLog>()).AsList();

        return new PagedResult<TradeLog> { Items = items, Page = metadata.Page, PageSize = pageSize, TotalCount = metadata.TotalCount };
    }

    public async Task<TradeLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT {Columns} FROM TradeLogs WHERE Id = @Id";
        await using var connection = context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TradeLog>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(TradeLog trade, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE TradeLogs SET
                UserCode = @UserCode, CryptoPair = @CryptoPair, TradeType = @TradeType,
                Price = @Price, Quantity = @Quantity, TotalUSD = @TotalUSD, FeeUSD = @FeeUSD,
                LocationCountry = @LocationCountry, ExecutionTimeMs = @ExecutionTimeMs,
                TransactionDate = @TransactionDate
            WHERE Id = @Id;
            """;
        trade.TotalUSD = Math.Round(trade.Price * trade.Quantity, 4);
        var feeRate = trade.Id % 20 == 0 ? 0.00075m : 0.001m;
        trade.FeeUSD = Math.Round(trade.TotalUSD * feeRate, 4);
        await using var connection = context.CreateConnection();
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, trade, cancellationToken: cancellationToken)) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = context.CreateConnection();
        return await connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM TradeLogs WHERE Id = @Id", new { Id = id }, cancellationToken: cancellationToken)) == 1;
    }

    private static void ApplyCoordinates(IEnumerable<CountryActivity> countries)
    {
        var coordinates = new Dictionary<string, (double Lat, double Lng)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Türkiye"] = (39.0, 35.0), ["ABD"] = (37.1, -95.7), ["Almanya"] = (51.2, 10.4),
            ["Birleşik Krallık"] = (55.4, -3.4), ["Japonya"] = (36.2, 138.3), ["Güney Kore"] = (36.5, 127.9),
            ["Singapur"] = (1.35, 103.8), ["Brezilya"] = (-14.2, -51.9), ["Kanada"] = (56.1, -106.3),
            ["Fransa"] = (46.2, 2.2), ["Hindistan"] = (20.6, 79.0), ["BAE"] = (23.4, 53.8)
        };
        foreach (var country in countries)
            if (coordinates.TryGetValue(country.LocationCountry, out var point))
                (country.Latitude, country.Longitude) = point;
    }

    private static decimal CalculatePercentageChange(decimal current, decimal previous)
        => previous == 0
            ? current == 0 ? 0 : 100
            : Math.Round((current - previous) * 100m / previous, 1);

    private sealed class PageMetadata
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
    }
}
