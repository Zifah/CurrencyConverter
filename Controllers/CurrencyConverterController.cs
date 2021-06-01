using CurrencyConverter.Models;
using CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Controllers
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
