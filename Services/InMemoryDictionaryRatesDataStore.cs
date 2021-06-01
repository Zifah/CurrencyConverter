using CurrencyConverter.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Services
{
    /// <inheritdoc/>
    public class InMemoryDictionaryRatesDataStore : IRatesDataStore
    {
        public static readonly IDictionary<DateTime, IDictionary<string, decimal>> ratesByDay = new ConcurrentDictionary<DateTime, IDictionary<string, decimal>>();

        /// <summary>
        /// Will be set each time data is saved to the data store
        /// </summary>
        public DateTime? LatestRatesDate { get; private set; }

        public InMemoryDictionaryRatesDataStore()
        {
        }

        /// <summary>
        /// <para>Will return the exchange rate to convert from <paramref name="sourceCurrency"/> and <paramref name="destinationCurrency"/> as at <paramref name="date"/></para>
        /// <para>Will return null if no exchange rate is found between these currencies for the specified date</para>
        /// </summary>
        /// <param name="sourceCurrency"></param>
        /// <param name="destinationCurrency"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public HistoricalRate GetConversionRate(string sourceCurrency, string destinationCurrency, DateTime date)
        {
            var latestRates = ratesByDay[date];

            decimal rate;
            var isForwardRateFound = latestRates.TryGetValue(GetExchangeRateKey(sourceCurrency, destinationCurrency), out rate);
            if (!isForwardRateFound)
            {
                if (latestRates.TryGetValue(GetExchangeRateKey(sourceCurrency, destinationCurrency, inverse: true), out rate))
                {
                    rate = 1 / rate;
                }
            }

            if (rate == 0)
            {
                return null;
            }

            return new HistoricalRate
            {
                Date = date,
                SourceCurrency = sourceCurrency,
                DestinationCurrency = destinationCurrency,
                Rate = rate
            };
        }

        public IDictionary<DateTime, decimal?> GetConversionRates(string sourceCurrency, string destinationCurrency, DateTime fromDate, DateTime toDate)
        {
            var result = new ConcurrentDictionary<DateTime, decimal?>();
            var exclusiveUpperIndex = (int)(toDate - fromDate).TotalDays + 1;
            Parallel.For(0, exclusiveUpperIndex, (int daysToAdd) =>
            {
                var currentDate = fromDate.AddDays(daysToAdd);
                result.TryAdd(currentDate, GetConversionRate(sourceCurrency, destinationCurrency, currentDate)?.Rate);
            });

            return result;
        }

        /// <summary>
        /// Save rates to the data store
        /// Update the value of <see cref="LatestRatesDate"/>
        /// </summary>
        /// <param name="latestRates"></param>
        public void SaveRates(IEnumerable<HistoricalRate> latestRates)
        {
            foreach (var rate in latestRates)
            {
                if (!ratesByDay.ContainsKey(rate.Date))
                {
                    ratesByDay[rate.Date] = new ConcurrentDictionary<string, decimal>();
                }

                ratesByDay[rate.Date][GetExchangeRateKey(rate.SourceCurrency, rate.DestinationCurrency)] = rate.Rate;
            }

            LatestRatesDate = ratesByDay.Max(x => x.Key);
        }

        private string GetExchangeRateKey(string sourceCurrency, string destinationCurrency, bool inverse = false)
        {
            return inverse ? $"{destinationCurrency}{sourceCurrency}" : $"{sourceCurrency}{destinationCurrency}";
        }
    }
}
