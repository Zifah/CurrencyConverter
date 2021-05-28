using CurrencyConverter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Services
{
    /// <summary>
    /// A source of currency conversion information and operations 
    /// </summary>
    public interface ICurrencyDataProvider
    {
        public string BaseCurrency { get; }

        /// <summary>
        /// Polls the primary source of currency information and brings local conversion rates store up-to-date
        /// </summary>
        /// <returns></returns>
        public Task RefreshConversionRatesAsync();

        /// <summary>
        /// Convert a value from one currency to another
        /// </summary>
        /// <param name="amount">The value (in source currency) to be converted</param>
        /// <param name="sourceCurrency">The source currency</param>
        /// <param name="destinationCurrency">The destination currency</param>
        /// <returns>The result of currency conversion</returns>
        /// <exception cref="ArgumentOutOfRangeException">Will throw if either <paramref name="sourceCurrency"/> or <paramref name="destinationCurrency"/> is not found</exception>
        public decimal Convert(decimal amount, string sourceCurrency, string destinationCurrency);

        /// <summary>
        /// Returns the conversion rate to convert a value from <paramref name="sourceCurrency"/> to <paramref name="destinationCurrency"/>
        /// </summary>
        /// <param name="sourceCurrency">The currency of the known value</param>
        /// <param name="destinationCurrency">The currency of the value to calculate using the returned rate</param>
        /// <returns>Rate: To convert a value in <paramref name="sourceCurrency"/> to <paramref name="destinationCurrency"/>, multiply it by this values</returns>
        public decimal GetConversionRate(string sourceCurrency, string destinationCurrency);

        /// <summary>
        /// Returns a conversation rate from over a range of dates up from a past date up to the present date
        /// </summary>
        /// <param name="fromDate">The earliest date for which rates are to be returned</param>
        /// <param name="toDate">The latest date for which rates are to be returned</param>
        /// <param name="sourceCurrency">The currency of the known value</param>
        /// <param name="destinationCurrency">The currency of the target value</param>
        /// <returns>A list of <see cref="HistoricalRate"/> objects with data for the requested duration</returns>
        public IList<HistoricalRate> GetHistoricalConversionRates(DateTime fromDate, DateTime toDate, string sourceCurrency, string destinationCurrency);
    }
}
