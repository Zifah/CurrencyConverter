using System;

namespace CurrencyConverter.Services
{
    public class DateProvider : IDateProvider
    {
        private readonly DateTime today;
        public DateProvider(DateTime today)
        {
            this.today = today;
        }

        public DateTime GetLastBusinessDayDate()
        {
            // Simplistic implementation. Will return the last week day
            // TODO: Factor in bank holidays
            DateTime result = today;
            bool isResultBusinessDay;
            do
            {
                result = result.AddDays(-1);
                isResultBusinessDay = result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday;
            } while (!isResultBusinessDay);

            return result;
        }
    }
}
