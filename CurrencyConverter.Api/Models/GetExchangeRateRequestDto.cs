using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Models
{
    public class GetExchangeRateRequestDto
    {
        [Required]
        public string SourceCurrency { get; set; }
        [Required]
        public string DestinationCurrency { get; set; }
        public DateTime? Date { get; set; }
    }
}
