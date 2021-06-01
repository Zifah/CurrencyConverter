using CurrencyConverter.Helpers;
using CurrencyConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Services
{
    /// <summary>
    /// Source of rates information: https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html
    /// </summary>
    public class EuroFxRefRatesProvider : ICurrencyDataProvider
    {
        public string BaseCurrency { get; } = "EUR";
        private readonly IRatesDataStore ratesDataStore;
        private readonly IThirdPartyRatesDataSource ratesDataSource;
        private static SemaphoreLocker semaphoreLocker = new SemaphoreLocker();

        public EuroFxRefRatesProvider(IRatesDataStore ratesDataStore, IThirdPartyRatesDataSource ratesDataSource)
        {
            this.ratesDataStore = Requires.NotNull(ratesDataStore, nameof(ratesDataStore));
            this.ratesDataSource = Requires.NotNull(ratesDataSource, nameof(ratesDataSource));
        }

        /// <inheritdoc/>
        public async Task<decimal> Convert(decimal amount, string sourceCurrency, string destinationCurrency)
        {
            // TODO: Create a default implementation of this method in the interface using the new C# 8.0 feature
            decimal rate = await GetConversionRate(sourceCurrency, destinationCurrency);
            return amount * rate;
        }

        /// <inheritdoc/>
        public async Task<decimal> GetConversionRate(string sourceCurrency, string destinationCurrency)
        {
            await EnsureRatesUpToDateAsync();
            HistoricalRate baseToSourceRate = ratesDataStore.GetConversionRate(BaseCurrency, sourceCurrency);
            HistoricalRate baseToDestinationRate = ratesDataStore.GetConversionRate(BaseCurrency, destinationCurrency);
            return GetConversationRate(baseToDestinationRate.Rate, baseToSourceRate.Rate);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<HistoricalRate>> GetHistoricalConversionRates(DateTime fromDate, DateTime toDate, string sourceCurrency, string destinationCurrency)
        {
            await EnsureRatesUpToDateAsync();

            var baseToSourceRates = ratesDataStore.GetConversionRates(BaseCurrency, sourceCurrency, fromDate, toDate).OrderBy(x => x.Key);
            IDictionary<DateTime, decimal?> baseToDestinationRates = ratesDataStore.GetConversionRates(BaseCurrency, destinationCurrency, fromDate, toDate);

            Stack<HistoricalRate> result = new Stack<HistoricalRate>();
            foreach (var sourceRate in baseToSourceRates)
            {
                DateTime rateDate = sourceRate.Key;
                var destinationRate = baseToDestinationRates[rateDate];
                if (sourceRate.Value.HasValue && destinationRate.HasValue)
                {
                    result.Push(new HistoricalRate
                    {
                        Date = rateDate,
                        SourceCurrency = sourceCurrency,
                        DestinationCurrency = destinationCurrency,
                        Rate = GetConversationRate(sourceRate.Value.Value, destinationRate.Value)
                    });
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task RefreshConversionRatesAsync()
        {
            await semaphoreLocker.LockAsync(async () =>
            {
                IEnumerable<HistoricalRate> latestRates = await ratesDataSource.GetRatesAfterAsync(ratesDataStore.LatestRatesDate);
                ratesDataStore.SaveRates(latestRates);
            });
        }

        private async Task EnsureRatesUpToDateAsync()
        {
            await RefreshConversionRatesAsync();

        }

        private decimal GetConversationRate(decimal baseToSourceRate, decimal baseToDestinationRate)
        {
            return baseToDestinationRate / baseToSourceRate;
        }
    }
}