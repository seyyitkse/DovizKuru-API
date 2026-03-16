using DovizKuru_API.Models;

namespace DovizKuru_API.Services
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateResponse> GetAllRatesAsync(string baseCurrency = "TRY");
        Task<ConversionResult> ConvertAsync(ConversionRequest request);
        Task<List<CurrencyInfo>> GetSupportedCurrenciesAsync();
    }
}
