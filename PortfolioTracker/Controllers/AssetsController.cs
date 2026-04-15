using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Data;
using PortfolioTracker.Models;
using PortfolioTracker.Services;

namespace PortfolioTracker.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CryptoService _cryptoService;

        public AssetsController(ApplicationDbContext context, CryptoService cryptoService)
        {
            _context = context;
            _cryptoService = cryptoService;
        }

        // GET: Assets - List all assets with optional filtering
        public async Task<IActionResult> Index(string? searchSymbol)
        {
            var assets = _context.Assets.AsQueryable();

            // Filter by symbol if provided
            if (!string.IsNullOrEmpty(searchSymbol))
            {
                assets = assets.Where(a => a.Symbol.Contains(searchSymbol.ToUpper()));
            }

            var assetList = await assets.OrderByDescending(a => a.PurchaseDate).ToListAsync();

            // Get current prices and build ViewModels
            var cryptos = await _cryptoService.GetTop50CryptocurrenciesAsync();
            var viewModels = new List<AssetViewModel>();

            foreach (var asset in assetList)
            {
                var crypto = cryptos.FirstOrDefault(c => c.Symbol.Equals(asset.Symbol, StringComparison.OrdinalIgnoreCase));
                var currentPrice = crypto?.CurrentPrice ?? 0;
                var previousPrice = _cryptoService.GetPriceChange(asset.Symbol);

                viewModels.Add(new AssetViewModel
                {
                    Id = asset.Id,
                    Symbol = asset.Symbol,
                    Quantity = asset.Quantity,
                    CurrentValue = currentPrice * asset.Quantity,
                    PurchaseDate = asset.PurchaseDate,
                    CurrentPrice = currentPrice,
                    PreviousPrice = previousPrice
                });
            }

            return View(viewModels);
        }

        // GET: Assets/Create
        public async Task<IActionResult> Create()
        {
            var cryptos = await _cryptoService.GetTop50CryptocurrenciesAsync();
            ViewBag.Cryptocurrencies = cryptos.Select(c => new { Value = c.Symbol.ToUpper(), Text = $"{c.Name} ({c.Symbol.ToUpper()})" }).ToList();
            return View();
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Symbol,Quantity,PurchaseDate")] Asset asset)
        {
            if (ModelState.IsValid)
            {
                // Get current price and calculate value
                var currentPrice = await _cryptoService.GetCurrentPriceAsync(asset.Symbol);
                if (currentPrice.HasValue && currentPrice.Value > 0)
                {
                    asset.CurrentValue = currentPrice.Value * asset.Quantity;
                }
                else
                {
                    ModelState.AddModelError("Symbol", $"Unable to fetch current price for {asset.Symbol}. Please try again.");
                    var cryptos = await _cryptoService.GetTop50CryptocurrenciesAsync();
                    ViewBag.Cryptocurrencies = cryptos.Select(c => new { Value = c.Symbol.ToUpper(), Text = $"{c.Name} ({c.Symbol.ToUpper()})" }).ToList();
                    return View(asset);
                }

                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var cryptos2 = await _cryptoService.GetTop50CryptocurrenciesAsync();
            ViewBag.Cryptocurrencies = cryptos2.Select(c => new { Value = c.Symbol.ToUpper(), Text = $"{c.Name} ({c.Symbol.ToUpper()})" }).ToList();
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            var cryptos = await _cryptoService.GetTop50CryptocurrenciesAsync();
            ViewBag.Cryptocurrencies = cryptos.Select(c => new { Value = c.Symbol.ToUpper(), Text = $"{c.Name} ({c.Symbol.ToUpper()})" }).ToList();
            return View(asset);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Symbol,Quantity,PurchaseDate")] Asset asset)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get current price and calculate value
                    var currentPrice = await _cryptoService.GetCurrentPriceAsync(asset.Symbol);
                    if (currentPrice.HasValue)
                    {
                        asset.CurrentValue = currentPrice.Value * asset.Quantity;
                    }
                    else
                    {
                        ModelState.AddModelError("Symbol", "Unable to fetch current price for this cryptocurrency.");
                        var cryptos = await _cryptoService.GetTop50CryptocurrenciesAsync();
                        ViewBag.Cryptocurrencies = cryptos.Select(c => new { Value = c.Symbol.ToUpper(), Text = $"{c.Name} ({c.Symbol.ToUpper()})" }).ToList();
                        return View(asset);
                    }

                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var cryptos2 = await _cryptoService.GetTop50CryptocurrenciesAsync();
            ViewBag.Cryptocurrencies = cryptos2.Select(c => new { Value = c.Symbol.ToUpper(), Text = $"{c.Name} ({c.Symbol.ToUpper()})" }).ToList();
            return View(asset);
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AssetExists(int id)
        {
            return _context.Assets.Any(e => e.Id == id);
        }
    }
}
