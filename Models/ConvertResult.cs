using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class ConvertResult
    {
        public decimal Result { get; set; }
        public decimal RateUsed { get; set; }
        public DateTime RateDate { get; set; }

    }
}
