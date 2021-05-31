using CurrencyConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Services
{
    /// <summary>
    /// Fetch rates data from the EuroFxRefRates website
    /// </summary>
    public class EuroFxRefRatesDataSource : IThirdPartyRatesDataSource
    {
        public EuroFxRefRatesDataSource()
        {
            // Inject that HttpClientFactory in here. That will be used to create the HTTP client
        }

        public Task<IEnumerable<HistoricalRate>> GetAllRatesAsync()
        {
            // Get the CSV file
            // Parse it into a list
            // Return the list
            throw new NotImplementedException();
        }

        public Task<IEnumerable<HistoricalRate>> GetCurrentRatesAsync()
        {
            // Get the PDF file
            // Extract today's rates data from the file
            // Return the data
            throw new NotImplementedException();
        }

        public Task<IEnumerable<HistoricalRate>> GetRatesAfterAsync(DateTime? exclusiveMinimumBoundDate)
        {
            // Fetch the whole file if the cache is empty or does not have the last working day's data and use this to populate the cache
            // If last update day was yesterday, fetch today's data through the PDF
            throw new NotImplementedException();
        }
    }
}
