using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Models;
using PortfolioTracker.Data;
using PortfolioTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PortfolioTracker.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CryptoService _cryptoService;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ApplicationDbContext context, CryptoService cryptoService, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _cryptoService = cryptoService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var dashboardData = await GetDashboardData(user.Id);
                return View("Dashboard", dashboardData);
            }
        }

        return View();
    }

    private async Task<DashboardViewModel> GetDashboardData(string userId)
    {
        var assets = await _context.Assets
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var dashboard = new DashboardViewModel
        {
            TotalAssets = assets.Count
        };

        if (!assets.Any())
        {
            return dashboard;
        }

        // Get current prices for all assets
        var symbols = assets.Select(a => a.Symbol).Distinct().ToList();
        var prices = await _cryptoService.GetCryptoPrices(symbols);

        decimal totalValue = 0;
        decimal totalCost = 0;

        foreach (var asset in assets)
        {
            if (prices.TryGetValue(asset.Symbol, out var price))
            {
                var currentValue = asset.Quantity * price;
                var purchasePricePerUnit = asset.CurrentValue / asset.Quantity; // CurrentValue stores total purchase cost
                var costBasis = purchasePricePerUnit * asset.Quantity; // Total amount paid

                totalValue += currentValue;
                totalCost += costBasis;

                dashboard.AssetAllocations.Add(new AssetAllocation
                {
                    Symbol = asset.Symbol,
                    Name = asset.Symbol, // Could be enhanced to get full name
                    Value = currentValue,
                    Percentage = 0 // Will calculate after total is known
                });
            }
        }

        dashboard.TotalPortfolioValue = totalValue;
        dashboard.TotalGainLoss = totalValue - totalCost;
        dashboard.TotalGainLossPercentage = totalCost > 0 ? (dashboard.TotalGainLoss / totalCost) * 100 : 0;

        // Calculate percentages
        foreach (var allocation in dashboard.AssetAllocations)
        {
            allocation.Percentage = totalValue > 0 ? (allocation.Value / totalValue) * 100 : 0;
        }

        // Get top performers (mock data for now - in real app would get from API)
        dashboard.TopPerformers = symbols.Take(5).Select(symbol => new TopPerformer
        {
            Symbol = symbol,
            Name = symbol,
            CurrentPrice = prices.GetValueOrDefault(symbol, 0),
            Change24h = 0, // Would need to get from API
            ChangePercentage24h = 0 // Would need to get from API
        }).ToList();

        // Generate portfolio history (mock data - would need historical data)
        dashboard.PortfolioHistory = GenerateMockHistory(totalValue);

        return dashboard;
    }

    private List<PortfolioHistoryPoint> GenerateMockHistory(decimal currentValue)
    {
        var history = new List<PortfolioHistoryPoint>();
        var random = new Random();
        var baseValue = currentValue * 0.8m; // Start 20% lower

        for (int i = 29; i >= 0; i--)
        {
            var date = DateTime.Now.AddDays(-i);
            var variation = (decimal)(random.NextDouble() * 0.1 - 0.05); // ±5% variation
            var value = baseValue * (1 + variation + (i / 30m * 0.25m)); // Trend upward

            history.Add(new PortfolioHistoryPoint
            {
                Date = date,
                Value = Math.Round(value, 2)
            });
        }

        return history;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
