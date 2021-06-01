using CurrencyConverter.Configuration;
using CurrencyConverter.Helpers;
using CurrencyConverter.Models;
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

namespace CurrencyConverter.Services
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
        public async Task<IEnumerable<HistoricalRate>> GetAllRatesAsync()
        {
            return await GetRatesAsync(config.HistoricalRatesCsvPath);
        }

        // https://www.ecb.europa.eu/stats/eurofxref/eurofxref.zip?104acdea95c84c086c74747ee81aa7e4
        public async Task<IEnumerable<HistoricalRate>> GetCurrentRatesAsync()
        {
            return await GetRatesAsync(config.LatestRatesCsvPath);
        }

        private async Task<IEnumerable<HistoricalRate>> GetRatesAsync(string ratesFileUri)
        {
            string ratesFileUri = getHistorical ? config.HistoricalRatesCsvUri : config.LatestRatesCsvUri;
            // Get the zipped file as a stream
            using var zipFileStream = await GetRatesZipFileAsync(ratesFileUri);

            // Save the zip file
            var zipFilePath = fileOperations.SaveStreamToTempFile(zipFileStream, $"{Guid.NewGuid()}.zip");

            // Decompress the zip file
            string unzipDirectory = fileOperations.UnzipArchive(zipFilePath);
            var ratesFilePath = Path.Combine(unzipDirectory, config.RatesFilePathInZip);

            // Parse it into a list
            using var csvFileStream = new StreamReader(ratesFilePath);
            IDictionary<DateTime, IDictionary<string, string>> ratesByDate = csvHelper.ParseText(csvFileStream);
            var result = new ConcurrentBag<HistoricalRate>();

            Parallel.ForEach(ratesByDate, kvp =>
            {
                var ratesDate = kvp.Key;
                foreach (KeyValuePair<string, string> rateKvp in kvp.Value)
                {
                    string destinationCurrency = rateKvp.Key;
                    string rateString = rateKvp.Value;
                    if (decimal.TryParse(rateString, out decimal rate))
                    {
                        result.Add(new HistoricalRate
                        {
                            Date = ratesDate,
                            SourceCurrency = BaseCurrency,
                            DestinationCurrency = rateKvp.Key,
                            Rate = rate
                        });
                    }
                    else
                    {
                        logger.LogInformation($"{BaseCurrency} to {destinationCurrency} rates for {ratesDate} is not valid. Value {rateString} is not a valid decimal");
                    }
                }
            });

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<HistoricalRate>> GetRatesAfterAsync(DateTime? exclusiveMinimumBoundDate)
        {
            DateTime lastBusinessDay = dateProvider.GetLastBusinessDayDate();

            Boolean doWeHaveSecondToCurrentData = exclusiveMinimumBoundDate == lastBusinessDay;

            if(doWeHaveSecondToCurrentData)
            {
                return await GetCurrentRatesAsync();
            }

            var allRates = await GetAllRatesAsync();
            return exclusiveMinimumBoundDate.HasValue ? allRates.Where(r => r.Date > exclusiveMinimumBoundDate) : allRates;
        }

        private async Task<Stream> GetRatesZipFileAsync(string path)
        {
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
