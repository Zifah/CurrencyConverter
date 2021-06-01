using CurrencyConverter.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Controllers
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

        // GET: from
        public async Task<ActionResult> Index(string sourceCurrency, string destinationCurrency)
        {
            var result = await exchangeRatesProvider.GetConversionRate(sourceCurrency, destinationCurrency);
            return Ok(result);
        }
    }
}
