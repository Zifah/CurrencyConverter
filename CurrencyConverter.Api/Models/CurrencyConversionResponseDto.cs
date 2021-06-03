using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Models
{
    public class CurrencyConversionResponseDto
    {
        [Required]
        public DateTime RateDate { get; set; }
        [Required]
        public decimal Rate { get; set; }
        [Required]
        public decimal Result { get; set; }

    }
}
