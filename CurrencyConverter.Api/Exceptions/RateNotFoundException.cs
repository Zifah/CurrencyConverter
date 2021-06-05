using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Exceptions
{
    public class RateNotFoundException : Exception
    {
        public RateNotFoundException()
        {
        }

        public RateNotFoundException(string message)
            : base(message)
        {
        }

        public RateNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RateNotFoundException(string sourceCurrency, string destinationCurrency, DateTime date) :
            this($"Could not find an exchange rate from {sourceCurrency} to {destinationCurrency} on {date}")
        {
        }
    }
}
