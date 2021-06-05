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
        [StringLength(3)]
        public string SourceCurrency { get; set; }

        [Required]
        [StringLength(3)]
        public string DestinationCurrency { get; set; }

        public DateTime? Date { get; set; }
    }
}
