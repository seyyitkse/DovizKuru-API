namespace DovizKuruAPI.Models
{
    public class CurrencyRate
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameTR { get; set; } = string.Empty;
        public decimal BuyingRate { get; set; }
        public decimal SellingRate { get; set; }
        public decimal Rate { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
    }

    public class ConversionRequest
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
    }

    public class ConversionResult
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Result { get; set; }
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ExchangeRateResponse
    {
        public string Base { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
        public List<CurrencyInfo> Currencies { get; set; } = new();
    }

    public class CurrencyInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameTR { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
    }
}
