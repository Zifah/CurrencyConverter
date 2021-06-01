using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class CurrencyConversionRequestDto
    {
        [Required]
        public string SourceCurrency { get; set; }
        [Required]
        public string DestinationCurrency { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
    }
}
