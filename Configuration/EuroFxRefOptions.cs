using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Configuration
{
    public class EuroFxRefOptions
    {
        public string BaseUrl { get; set; }
        public string LatestRatesCsvPath { get; set; }
        public string HistoricalRatesCsvPath { get; set; }
        public string RatesFilePathInZip { get; set; }
    }
}
