using System;

namespace CurrencyConverter.Services
{
    /// <summary>
    /// Date provider
    /// </summary>
    public interface IDateProvider
    {
        /// <summary>
        /// Return the date of the last business date before the current day
        /// </summary>
        /// <returns></returns>
        DateTime GetLastBusinessDayDate();

        /// <summary>
        /// <para>Return the date of the most recent business date</para>
        /// <para>If <paramref name="asAt"/> is specified, the latest business day on or before its value will be returned</para>
        /// </summary>
        /// <param name="asAt">If this date value is specified, the latest business day on or before the value will be returned</param>
        /// <returns></returns>
        DateTime GetCurrentBusinessDayDate(DateTime? asAt = null);
    }
}