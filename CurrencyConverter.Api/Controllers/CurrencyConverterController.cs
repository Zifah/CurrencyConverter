using CurrencyConverter.Api.Models;
using CurrencyConverter.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ICurrencyDataProvider exchangeRatesProvider;

        public CurrencyConverterController(
            ICurrencyDataProvider exchangeRatesProvider
            ) : base()
        {
            this.exchangeRatesProvider = Requires.NotNull(exchangeRatesProvider, nameof(exchangeRatesProvider));
        }

        [HttpPost]
        public async Task<ActionResult> Post(CurrencyConversionRequestDto requestData)
        {
            var convertResult = await exchangeRatesProvider.Convert(requestData.Amount, requestData.SourceCurrency, requestData.DestinationCurrency, requestData.Date);
            var result = new CurrencyConversionResponseDto
            {
                RateDate = convertResult.RateDate,
                Rate = convertResult.RateUsed,
                Result = convertResult.Result
            };
            return Ok(result);
        }
    }
}
