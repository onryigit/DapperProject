namespace DapperProject.Models;

public sealed class DashboardViewModel
{
    public DashboardSummary Summary { get; init; } = new();
    public IReadOnlyList<VolumePoint> VolumeTrend { get; init; } = [];
    public IReadOnlyList<PairDistribution> PairDistribution { get; init; } = [];
    public IReadOnlyList<TradeTypeDistribution> TradeTypeDistribution { get; init; } = [];
    public IReadOnlyList<TradeLog> TopTransactions { get; init; } = [];
    public IReadOnlyList<CountryActivity> CountryActivity { get; init; } = [];
    public decimal BuyPressure { get; init; }
    public decimal ServerCapacityUsage { get; init; }
}

public sealed class DashboardSummary
{
    public decimal TotalVolume { get; set; }
    public decimal TotalFees { get; set; }
    public string HighestVolumePair { get; set; } = "—";
    public decimal AverageExecutionTime { get; set; }
    public int TotalTrades { get; set; }
}

public sealed class VolumePoint
{
    public DateTime TradeDate { get; set; }
    public decimal Volume { get; set; }
}

public sealed class PairDistribution
{
    public string CryptoPair { get; set; } = string.Empty;
    public long TradeCount { get; set; }
    public decimal Volume { get; set; }
}

public sealed class TradeTypeDistribution
{
    public string TradeType { get; set; } = string.Empty;
    public long TradeCount { get; set; }
}

public sealed class CountryActivity
{
    public string LocationCountry { get; set; } = string.Empty;
    public long TradeCount { get; set; }
    public decimal Volume { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
