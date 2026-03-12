using DovizKuruAPI.Models;

namespace DovizKuruAPI.Services
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateResponse> GetAllRatesAsync(string baseCurrency = "TRY");
        Task<ConversionResult> ConvertAsync(ConversionRequest request);
        Task<List<CurrencyInfo>> GetSupportedCurrenciesAsync();
    }
}
