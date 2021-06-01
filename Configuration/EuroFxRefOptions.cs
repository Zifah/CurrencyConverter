using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Configuration
{
    public class EuroFxRefOptions
    {
        public string BaseUrl { get; set; }
        public string LatestRatesCsvUri { get; set; }
        public string HistoricalRatesCsvUri { get; set; }
        public string LatestRatesFilePathInZip { get; set; }
        public string HistoricalRatesFilePathInZip { get; set; }
    }
}
