using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class GetExchangeRateRequestDto
    {
        public string SourceCurrency { get; set; }
        public string DestinationCurrency { get; set; }
        public DateTime? Date { get; set; }
    }
}
