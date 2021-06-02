using Microsoft.Extensions.Options;
using System;
using CurrencyConverter.Configuration;
using Validation;

namespace CurrencyConverter.Services
{
    public class DateProvider : IDateProvider
    {
        private readonly DateTime now;
        private readonly GeneralOptions generalConfig;
        public DateProvider(DateTime now, IOptions<GeneralOptions> config)
        {
            this.now = now;
            generalConfig = Requires.NotNull(config, nameof(config)).Value;
        }

        /// <inheritdoc/>
        public DateTime GetLastBusinessDayDate()
        {
            return GetCurrentBusinessDayDate(now.Date.AddDays(-1));
        }

        /// <inheritdoc/>
        public DateTime GetCurrentBusinessDayDate(DateTime? asAt = null)
        {
            if (asAt.HasValue)
            {
                return GetLastBusinessDayOnOrBefore(asAt.Value);
            }
            else if (now.TimeOfDay >= TimeSpan.Parse(generalConfig.BusinessDayStart))
            {
                return GetLastBusinessDayOnOrBefore(now.Date);
            }
            else
            {
                return GetLastBusinessDayOnOrBefore(now.Date.AddDays(-1));
            }
        }

        private DateTime GetLastBusinessDayOnOrBefore(DateTime date)
        {
            // Simplistic implementation. Will return the last week day
            // TODO: Factor in bank holidays
            DateTime result = date;

            while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday)
            {
                result = result.AddDays(-1);
            }

            return result;
        }
    }
}
