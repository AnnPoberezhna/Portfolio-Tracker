using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace PortfolioTracker.Services
{
    public class CryptoService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CryptoService> _logger;
        private static readonly SemaphoreSlim _rateLimiter = new SemaphoreSlim(3, 3); // Max 3 concurrent requests
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(20); // 3 per minute = 1 every 20 seconds
        private static readonly Dictionary<string, decimal> _currentPrices = new Dictionary<string, decimal>();
        private static readonly Dictionary<string, decimal> _previousPrices = new Dictionary<string, decimal>();

        public CryptoService(HttpClient httpClient, IMemoryCache cache, ILogger<CryptoService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<CryptoCurrency>> GetTop50CryptocurrenciesAsync()
        {
            const string cacheKey = "Top50Cryptos";
            if (_cache.TryGetValue(cacheKey, out List<CryptoCurrency> cachedCryptos))
            {
                return cachedCryptos;
            }

            await _rateLimiter.WaitAsync();
            try
            {
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                if (timeSinceLastRequest < _minRequestInterval)
                {
                    await Task.Delay(_minRequestInterval - timeSinceLastRequest);
                }

                _lastRequestTime = DateTime.UtcNow;

                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=50&page=1&sparkline=false");
                request.Headers.Add("User-Agent", "PortfolioTracker/1.0");
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var cryptos = JsonSerializer.Deserialize<List<CryptoCurrency>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Update price history using the last known cached price as the previous price
                if (cryptos != null)
                {
                    foreach (var crypto in cryptos)
                    {
                        if (_currentPrices.TryGetValue(crypto.Symbol, out var lastPrice))
                        {
                            _previousPrices[crypto.Symbol] = lastPrice;
                        }
                        _currentPrices[crypto.Symbol] = crypto.CurrentPrice;
                    }
                }

                _cache.Set(cacheKey, cryptos, TimeSpan.FromSeconds(20)); // Cache for 20 seconds
                return cryptos ?? new List<CryptoCurrency>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cryptocurrencies from CoinGecko");
                return new List<CryptoCurrency>();
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        public decimal GetPriceChange(string symbol)
        {
            if (_previousPrices.TryGetValue(symbol, out var previousPrice))
            {
                return previousPrice;
            }
            return 0;
        }

        public async Task<decimal?> GetCurrentPriceAsync(string symbol)
        {
            try
            {
                var cryptos = await GetTop50CryptocurrenciesAsync();
                _logger.LogInformation($"Fetched {cryptos.Count} cryptocurrencies from cache");
                
                var crypto = cryptos.FirstOrDefault(c => c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
                if (crypto != null)
                {
                    _logger.LogInformation($"Found {symbol}: Price = {crypto.CurrentPrice}");
                    return crypto.CurrentPrice;
                }
                else
                {
                    _logger.LogWarning($"Cryptocurrency {symbol} not found in top 50");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting price for {symbol}");
                return null;
            }
        }
    }

    public class CryptoCurrency
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        
        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }
        
        public long MarketCap { get; set; }
        public int MarketCapRank { get; set; }
    }
}