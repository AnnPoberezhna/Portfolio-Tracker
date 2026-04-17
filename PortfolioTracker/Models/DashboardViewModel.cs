using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Models;

public class DashboardViewModel
{
    public decimal TotalPortfolioValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal TotalGainLossPercentage { get; set; }
    public int TotalAssets { get; set; }
    public List<AssetAllocation> AssetAllocations { get; set; } = new();
    public List<PortfolioHistoryPoint> PortfolioHistory { get; set; } = new();
    public List<TopPerformer> TopPerformers { get; set; } = new();
}

public class AssetAllocation
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
}

public class PortfolioHistoryPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}

public class TopPerformer
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal Change24h { get; set; }
    public decimal ChangePercentage24h { get; set; }
}