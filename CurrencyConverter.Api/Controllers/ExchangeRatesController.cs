using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly ICurrencyDataProvider exchangeRatesProvider;

        public ExchangeRatesController(
            ICurrencyDataProvider exchangeRatesProvider
            ) : base()
        {
            this.exchangeRatesProvider = exchangeRatesProvider;
        }

        // GET
        [HttpGet]
        public async Task<ActionResult> Index([FromQuery]GetExchangeRateRequestDto exchangeRateRequest)
        {
            var result = await exchangeRatesProvider.GetConversionRate(
                exchangeRateRequest.SourceCurrency, 
                exchangeRateRequest.DestinationCurrency,
                exchangeRateRequest.Date);
            return Ok(result);
        }
    }
}
