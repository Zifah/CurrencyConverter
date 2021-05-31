using CurrencyConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Services
{
    public interface IRatesDataStore
    {
        /// <summary>
        /// Gets the most recent date for which rates exist in the application storage
        /// </summary>
        /// <returns></returns>
        DateTime? LatestRatesDate { get; }

        /// <summary>
        /// Gets the latest exchange rate to convert a value from <paramref name="sourceCurrency"/> to <paramref name="destinationCurrency"/> 
        /// </summary>
        /// <param name="sourceCurrency"></param>
        /// <param name="destinationCurrency"></param>
        /// <returns></returns>
        HistoricalRate GetConversionRate(string sourceCurrency, string destinationCurrency);

        /// <summary>
        /// Get all exchange rates for dates between <paramref name="fromDate"/> and <paramref name="toDate"/>
        /// </summary>
        /// <param name="sourceCurrency"></param>
        /// <param name="destinationCurrency"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        IDictionary<DateTime, decimal?> GetConversionRates(string sourceCurrency, string destinationCurrency, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Save exchange rates
        /// </summary>
        /// <param name="latestRates"></param>
        void SaveRates(IEnumerable<HistoricalRate> latestRates);
    }
}
