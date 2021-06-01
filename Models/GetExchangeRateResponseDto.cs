using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class GetExchangeRateResponseDto
    {
        public string SourceCurrency { get; set; }
        public string DestinationCurrency { get; set; }
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}
