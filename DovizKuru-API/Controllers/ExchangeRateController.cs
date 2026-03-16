using System;
using DovizKuru_API.Models;
using DovizKuru_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DovizKuru_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRateController> _logger;

        public ExchangeRateController(IExchangeRateService exchangeRateService, ILogger<ExchangeRateController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpGet("rates")]
        public async Task<ActionResult<ExchangeRateResponse>> GetRates([FromQuery] string baseCurrency = "TRY")
        {
            try
            {
                var rates = await _exchangeRateService.GetAllRatesAsync(baseCurrency);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kurlar getirilirken hata oluştu");
                return StatusCode(500, "Kurlar alınırken bir hata oluştu");
            }
        }

        [HttpPost("convert")]
        public async Task<ActionResult<ConversionResult>> Convert([FromBody] ConversionRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return BadRequest("Miktar 0'dan büyük olmalıdır");
                }

                var result = await _exchangeRateService.ConvertAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dönüşüm yapılırken hata oluştu");
                return StatusCode(500, "Dönüşüm yapılırken bir hata oluştu");
            }
        }

        [HttpGet("convert")]
        public async Task<ActionResult<ConversionResult>> ConvertGet([FromQuery] decimal amount, [FromQuery] string from, [FromQuery] string to)
        {
            var request = new ConversionRequest
            {
                Amount = amount,
                FromCurrency = from,
                ToCurrency = to
            };

            return await Convert(request);
        }

        [HttpGet("currencies")]
        public async Task<ActionResult<List<CurrencyInfo>>> GetCurrencies()
        {
            var currencies = await _exchangeRateService.GetSupportedCurrenciesAsync();
            return Ok(currencies);
        }
    }
}
