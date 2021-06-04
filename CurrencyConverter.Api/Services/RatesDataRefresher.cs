using CurrencyConverter.Api.Helpers;
using CurrencyConverter.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Api.Services
{
    public class RatesDataRefresher
    {
        private static SemaphoreLocker semaphoreLocker = new SemaphoreLocker();
        private readonly IRatesDataStore ratesDataStore;
        private readonly IThirdPartyRatesDataSource ratesDataSource;
        private readonly IDateProvider dateProvider;
        private readonly DateTime currentBusinessDay;

        public RatesDataRefresher(
            IRatesDataStore ratesDataStore,
            IThirdPartyRatesDataSource ratesDataSource,
            IDateProvider dateProvider)
        {
            this.dateProvider = dateProvider;
            this.ratesDataStore = Requires.NotNull(ratesDataStore, nameof(ratesDataStore));
            this.ratesDataSource = Requires.NotNull(ratesDataSource, nameof(ratesDataSource));
            currentBusinessDay = this.dateProvider.GetCurrentBusinessDayDate();

        }



        /// <summary>
        /// Polls the primary source of currency information and brings local conversion rates store up-to-date
        /// </summary>
        /// <returns></returns>
        public async Task RefreshConversionRatesAsync()
        {
            await semaphoreLocker.LockAsync(async () =>
            {
                if (ratesDataStore.LatestRatesDate >= currentBusinessDay)
                {
                    return;
                }

                IDictionary<DateTime, IEnumerable<HistoricalRate>> latestRates = await ratesDataSource.GetRatesAfterAsync(ratesDataStore.LatestRatesDate);
                ratesDataStore.SaveRates(latestRates);
            });
        }
    }
}
