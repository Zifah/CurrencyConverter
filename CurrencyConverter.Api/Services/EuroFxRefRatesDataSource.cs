using CurrencyConverter.Api.Configuration;
using CurrencyConverter.Api.Helpers;
using CurrencyConverter.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Api.Services
{
    /// <summary>
    /// Fetch rates data from the EuroFxRefRates website
    /// </summary>
    public class EuroFxRefRatesDataSource : IThirdPartyRatesDataSource
    {
        public string BaseCurrency { get; } = "EUR";
        private readonly HttpClient httpClient;
        private readonly EuroFxCsvHelper csvHelper;
        private readonly IFileOperations fileOperations;
        private readonly EuroFxRefOptions config;
        private readonly ILogger<EuroFxRefRatesDataSource> logger;
        private readonly IDateProvider dateProvider;

        public EuroFxRefRatesDataSource(
            IHttpClientFactory httpClientFactory,
            IFileOperations fileOperations,
            EuroFxCsvHelper csvHelper,
            IOptions<EuroFxRefOptions> config,
            ILogger<EuroFxRefRatesDataSource> logger,
            IDateProvider dateProvider)
        {
            // Inject that HttpClientFactory in here. That will be used to create the HTTP client
            var httpFactory = Requires.NotNull(httpClientFactory, nameof(httpClientFactory));
            this.httpClient = httpFactory.CreateClient(ApplicationConstants.EuroFxRefNamedClientName);
            this.csvHelper = Requires.NotNull(csvHelper, nameof(csvHelper));
            this.fileOperations = Requires.NotNull(fileOperations, nameof(FileOperations));
            this.config = Requires.NotNull(config.Value, nameof(config));
            this.logger = Requires.NotNull(logger, nameof(logger));
            this.dateProvider = Requires.NotNull(dateProvider, nameof(dateProvider));
        }

        // https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.zip?104acdea95c84c086c74747ee81aa7e4
        public async Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetAllRatesAsync()
        {
            return await GetRatesAsync(true);
        }

        // https://www.ecb.europa.eu/stats/eurofxref/eurofxref.zip?104acdea95c84c086c74747ee81aa7e4
        public async Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetCurrentRatesAsync()
        {
            return await GetRatesAsync(false);
        }

        private async Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetRatesAsync(bool getHistorical)
        {
            string ratesFileUri = getHistorical ? config.HistoricalRatesCsvUri : config.LatestRatesCsvUri;
            // Get the zipped file as a stream
            using var zipFileStream = await GetRatesZipFileAsync(ratesFileUri);

            // Save the zip file
            var zipFilePath = fileOperations.SaveStreamToTempFile(zipFileStream, $"{Guid.NewGuid()}.zip");

            // Decompress the zip file
            string unzipDirectory = fileOperations.UnzipArchive(zipFilePath);
            string csvFilePathInZip = getHistorical ? config.HistoricalRatesFilePathInZip : config.LatestRatesFilePathInZip;
            var ratesFilePath = Path.Combine(unzipDirectory, csvFilePathInZip);

            // Parse it into a list
            using var csvFileStream = new StreamReader(ratesFilePath);
            IDictionary<DateTime, IDictionary<string, decimal?>> ratesByDate = csvHelper.ParseText(csvFileStream);
            var result = new ConcurrentDictionary<DateTime, IEnumerable<HistoricalRate>>();
            var baseToBaseRate = new HistoricalRate
            {
                Rate = 1,
                SourceCurrency = BaseCurrency,
                DestinationCurrency = BaseCurrency
            };

            Parallel.ForEach(ratesByDate, kvp =>
            {
                var theDate = kvp.Key;
                var theRates = kvp.Value;

                var dateRates = new ConcurrentBag<HistoricalRate>();
                Parallel.ForEach(theRates, currencyRate =>
                {
                    string destinationCurrency = currencyRate.Key;
                    decimal? rateValue = currencyRate.Value;
                    if (rateValue.HasValue)
                    {
                        dateRates.Add(new HistoricalRate
                        {
                            SourceCurrency = BaseCurrency,
                            DestinationCurrency = destinationCurrency,
                            Rate = rateValue.Value
                        });
                    }
                });

                // Add the Base currency to Base currency rate which is always 1
                dateRates.Add(baseToBaseRate);

                result[theDate] = dateRates;
            });

            return result;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetRatesAfterAsync(DateTime? exclusiveMinimumBoundDate)
        {
            DateTime lastBusinessDay = dateProvider.GetLastBusinessDayDate();

            bool doWeHaveSecondToCurrentData = exclusiveMinimumBoundDate == lastBusinessDay;

            if (doWeHaveSecondToCurrentData)
            {
                return await GetCurrentRatesAsync();
            }

            var allRates = await GetAllRatesAsync();

            return exclusiveMinimumBoundDate.HasValue ?
                allRates
                .Where(r => r.Key > exclusiveMinimumBoundDate)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) :
                allRates;
        }

        private async Task<Stream> GetRatesZipFileAsync(string path)
        {
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
