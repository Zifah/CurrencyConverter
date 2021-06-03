﻿using CurrencyConverter.Api.Exceptions;
using CurrencyConverter.Api.Helpers;
using CurrencyConverter.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Api.Services
{
    /// <summary>
    /// Source of rates information: https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html
    /// </summary>
    public class EuroFxRefRatesProvider : ICurrencyDataProvider
    {
        public string BaseCurrency { get; }
        private readonly IRatesDataStore ratesDataStore;
        private readonly IThirdPartyRatesDataSource ratesDataSource;
        private readonly IDateProvider dateProvider;
        private readonly DateTime currentBusinessDay;
        private static SemaphoreLocker semaphoreLocker = new SemaphoreLocker();

        public EuroFxRefRatesProvider(
            IRatesDataStore ratesDataStore,
            IThirdPartyRatesDataSource ratesDataSource,
            IDateProvider dateProvider)
        {
            this.ratesDataStore = Requires.NotNull(ratesDataStore, nameof(ratesDataStore));
            this.ratesDataSource = Requires.NotNull(ratesDataSource, nameof(ratesDataSource));
            this.dateProvider = dateProvider;
            this.currentBusinessDay = dateProvider.GetCurrentBusinessDayDate();
            BaseCurrency = Requires.NotNull(ratesDataSource.BaseCurrency, nameof(ratesDataSource.BaseCurrency));
        }

        /// <inheritdoc/>
        public async Task<ConvertResult> Convert(decimal amount, string sourceCurrency, string destinationCurrency, DateTime? date)
        {
            decimal rate;
            if (date.HasValue)
            {
                HistoricalRate rateObj = (await GetHistoricalConversionRates(date.Value, date.Value, sourceCurrency, destinationCurrency)).SingleOrDefault();

                if (rateObj == null)
                {
                    throw new RateNotFoundException(sourceCurrency, destinationCurrency);
                }

                rate = rateObj.Rate;
            }
            else
            {
                rate = await GetConversionRate(sourceCurrency, destinationCurrency);
            }

            return new ConvertResult
            {
                Result = amount * rate,
                RateUsed = rate,
                RateDate = date.HasValue ? dateProvider.GetCurrentBusinessDayDate(date.Value) : currentBusinessDay
            };
        }

        /// <inheritdoc/>
        public async Task<decimal> GetConversionRate(string sourceCurrency, string destinationCurrency)
        {
            await RefreshConversionRatesAsync();
            HistoricalRate baseToSourceRate = ratesDataStore.GetConversionRate(BaseCurrency, sourceCurrency, currentBusinessDay);

            if (baseToSourceRate == null)
            {
                throw new RateNotFoundException(sourceCurrency, destinationCurrency);
            }

            HistoricalRate baseToDestinationRate = ratesDataStore.GetConversionRate(BaseCurrency, destinationCurrency, currentBusinessDay);
            if (baseToDestinationRate == null)
            {
                throw new RateNotFoundException(sourceCurrency, destinationCurrency);
            }

            return GetConversionRate(baseToSourceRate.Rate, baseToDestinationRate.Rate);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<HistoricalRate>> GetHistoricalConversionRates(DateTime fromDate, DateTime toDate, string sourceCurrency, string destinationCurrency)
        {
            await RefreshConversionRatesAsync();

            // For scenarios where from date falls on a weekend, we want to fetch the rates on the most recent business date before it
            DateTime searchFromDate = dateProvider.GetCurrentBusinessDayDate(fromDate);

            var baseToSourceRates = ratesDataStore.GetConversionRates(BaseCurrency, sourceCurrency, searchFromDate, toDate);
            IDictionary<DateTime, decimal?> baseToDestinationRates = ratesDataStore.GetConversionRates(BaseCurrency, destinationCurrency, fromDate, toDate);

            Stack<HistoricalRate> result = new Stack<HistoricalRate>();

            var currentDate = fromDate;
            while (currentDate <= toDate)
            {
                var currentBusinessDate = dateProvider.GetCurrentBusinessDayDate(currentDate);
                decimal? baseToSourceRate = baseToSourceRates[currentBusinessDate];
                decimal? baseToDestinationRate = baseToDestinationRates[currentBusinessDate];
                if (baseToSourceRate.HasValue && baseToDestinationRate.HasValue)
                {
                    result.Push(new HistoricalRate
                    {
                        Date = currentDate,
                        SourceCurrency = sourceCurrency,
                        DestinationCurrency = destinationCurrency,
                        Rate = GetConversionRate(baseToSourceRate.Value, baseToDestinationRate.Value)
                    });
                }
                currentDate.AddDays(1);
            }

            return result;
        }

        /// <inheritdoc/>
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

        private decimal GetConversionRate(decimal baseToSourceRate, decimal baseToDestinationRate)
        {
            return baseToDestinationRate / baseToSourceRate;
        }
    }
}