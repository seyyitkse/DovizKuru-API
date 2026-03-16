using DovizKuru_API.Models;
using System.Text.Json;

namespace DovizKuru_API.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeRateService> _logger;
        private Dictionary<string, decimal> _cachedRates = new();
        private DateTime _lastUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        private readonly List<CurrencyInfo> _currencies = new()
        {
            new() { Code = "TRY", Name = "Turkish Lira", NameTR = "Türk Lirasý", Symbol = "?", Flag = "????" },
            new() { Code = "USD", Name = "US Dollar", NameTR = "Amerikan Dolarý", Symbol = "$", Flag = "????" },
            new() { Code = "EUR", Name = "Euro", NameTR = "Euro", Symbol = "€", Flag = "????" },
            new() { Code = "GBP", Name = "British Pound", NameTR = "Ýngiliz Sterlini", Symbol = "£", Flag = "????" },
            new() { Code = "CHF", Name = "Swiss Franc", NameTR = "Ýsviçre Frangý", Symbol = "?", Flag = "????" },
            new() { Code = "JPY", Name = "Japanese Yen", NameTR = "Japon Yeni", Symbol = "¥", Flag = "????" },
            new() { Code = "AUD", Name = "Australian Dollar", NameTR = "Avustralya Dolarý", Symbol = "A$", Flag = "????" },
            new() { Code = "CAD", Name = "Canadian Dollar", NameTR = "Kanada Dolarý", Symbol = "C$", Flag = "????" },
            new() { Code = "CNY", Name = "Chinese Yuan", NameTR = "Çin Yuaný", Symbol = "¥", Flag = "????" },
            new() { Code = "RUB", Name = "Russian Ruble", NameTR = "Rus Rublesi", Symbol = "?", Flag = "????" },
            new() { Code = "SAR", Name = "Saudi Riyal", NameTR = "Suudi Riyali", Symbol = "?", Flag = "????" },
            new() { Code = "AED", Name = "UAE Dirham", NameTR = "BAE Dirhemi", Symbol = "?.?", Flag = "????" },
            new() { Code = "SEK", Name = "Swedish Krona", NameTR = "Ýsveç Kronu", Symbol = "kr", Flag = "????" },
            new() { Code = "NOK", Name = "Norwegian Krone", NameTR = "Norveç Kronu", Symbol = "kr", Flag = "????" },
            new() { Code = "DKK", Name = "Danish Krone", NameTR = "Danimarka Kronu", Symbol = "kr", Flag = "????" },
            new() { Code = "INR", Name = "Indian Rupee", NameTR = "Hindistan Rupisi", Symbol = "?", Flag = "????" },
            new() { Code = "KRW", Name = "South Korean Won", NameTR = "Güney Kore Wonu", Symbol = "?", Flag = "????" },
            new() { Code = "PLN", Name = "Polish Zloty", NameTR = "Polonya Zlotisi", Symbol = "z?", Flag = "????" },
            new() { Code = "BRL", Name = "Brazilian Real", NameTR = "Brezilya Reali", Symbol = "R$", Flag = "????" },
            new() { Code = "MXN", Name = "Mexican Peso", NameTR = "Meksika Pesosu", Symbol = "$", Flag = "????" },
            new() { Code = "ZAR", Name = "South African Rand", NameTR = "Güney Afrika Randý", Symbol = "R", Flag = "????" },
            new() { Code = "SGD", Name = "Singapore Dollar", NameTR = "Singapur Dolarý", Symbol = "S$", Flag = "????" },
            new() { Code = "HKD", Name = "Hong Kong Dollar", NameTR = "Hong Kong Dolarý", Symbol = "HK$", Flag = "????" },
            new() { Code = "NZD", Name = "New Zealand Dollar", NameTR = "Yeni Zelanda Dolarý", Symbol = "NZ$", Flag = "????" },
        };

        public ExchangeRateService(HttpClient httpClient, ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ExchangeRateResponse> GetAllRatesAsync(string baseCurrency = "TRY")
        {
            await RefreshRatesIfNeeded();

            var response = new ExchangeRateResponse
            {
                Base = baseCurrency,
                Date = DateTime.Now,
                Rates = new Dictionary<string, decimal>(),
                Currencies = _currencies
            };

            if (!_cachedRates.ContainsKey(baseCurrency.ToUpper()))
            {
                return response;
            }

            decimal baseRate = _cachedRates[baseCurrency.ToUpper()];

            foreach (var currency in _currencies)
            {
                if (_cachedRates.TryGetValue(currency.Code, out decimal rate))
                {
                    response.Rates[currency.Code] = rate / baseRate;
                }
            }

            return response;
        }

        public async Task<ConversionResult> ConvertAsync(ConversionRequest request)
        {
            await RefreshRatesIfNeeded();

            var result = new ConversionResult
            {
                Amount = request.Amount,
                FromCurrency = request.FromCurrency.ToUpper(),
                ToCurrency = request.ToCurrency.ToUpper(),
                Timestamp = DateTime.Now
            };

            if (!_cachedRates.TryGetValue(request.FromCurrency.ToUpper(), out decimal fromRate) ||
                !_cachedRates.TryGetValue(request.ToCurrency.ToUpper(), out decimal toRate))
            {
                throw new ArgumentException("Geçersiz para birimi");
            }

            result.Rate = toRate / fromRate;
            result.Result = Math.Round(request.Amount * result.Rate, 4);

            return result;
        }

        public Task<List<CurrencyInfo>> GetSupportedCurrenciesAsync()
        {
            return Task.FromResult(_currencies);
        }

        private async Task RefreshRatesIfNeeded()
        {
            if (DateTime.Now - _lastUpdate < _cacheExpiration && _cachedRates.Count > 0)
            {
                return;
            }

            try
            {
                var response = await _httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USD");
                var data = JsonSerializer.Deserialize<ExchangeRateApiResponse>(response);

                if (data?.rates != null)
                {
                    _cachedRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    foreach (var rate in data.rates)
                    {
                        _cachedRates[rate.Key.ToUpper()] = rate.Value;
                    }
                    _lastUpdate = DateTime.Now;
                    _logger.LogInformation("Döviz kurlarý güncellendi: {Count} para birimi", _cachedRates.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döviz kurlarý güncellenirken hata oluþtu");

                if (_cachedRates.Count == 0)
                {
                    _cachedRates = GetFallbackRates();
                }
            }
        }

        private Dictionary<string, decimal> GetFallbackRates()
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["USD"] = 1m,
                ["TRY"] = 32.5m,
                ["EUR"] = 0.92m,
                ["GBP"] = 0.79m,
                ["CHF"] = 0.88m,
                ["JPY"] = 154m,
                ["AUD"] = 1.53m,
                ["CAD"] = 1.36m,
                ["CNY"] = 7.24m,
                ["RUB"] = 92m,
                ["SAR"] = 3.75m,
                ["AED"] = 3.67m,
                ["SEK"] = 10.8m,
                ["NOK"] = 10.9m,
                ["DKK"] = 6.9m,
                ["INR"] = 83.5m,
                ["KRW"] = 1350m,
                ["PLN"] = 4.0m,
                ["BRL"] = 5.0m,
                ["MXN"] = 17m,
                ["ZAR"] = 18.5m,
                ["SGD"] = 1.34m,
                ["HKD"] = 7.8m,
                ["NZD"] = 1.65m,
            };
        }

        private class ExchangeRateApiResponse
        {
            public string? @base { get; set; }
            public Dictionary<string, decimal>? rates { get; set; }
        }
    }
}
