using CurrencyConverter.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Services
{
    /// <summary>
    /// Interface through which exchange rates data can be fetched from a third party provider
    /// </summary>
    public interface IThirdPartyRatesDataSource
    {
        string BaseCurrency { get; }

        /// <summary>
        /// Gets the most up-to-date exchange rates i.e. exchange rates for the last/current business day
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetCurrentRatesAsync();

        /// <summary>
        /// Fetch all exchange rates from the provider from inception
        /// </summary>
        /// <returns></returns>
        Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetAllRatesAsync();

        /// <summary>
        /// Fetch all exchange rates from the provider for dates after <paramref name="exclusiveMinimumBoundDate"/>
        /// If the value of <paramref name="exclusiveMinimumBoundDate"/> is null, then all rates from inception will be fetched
        /// </summary>
        /// <param name="exclusiveMinimumBoundDate">Only exchange rates for days after this data will be returned</param>
        /// <returns></returns>
        Task<IDictionary<DateTime, IEnumerable<HistoricalRate>>> GetRatesAfterAsync(DateTime? exclusiveMinimumBoundDate);
    }
}