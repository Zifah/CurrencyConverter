using System;

namespace CurrencyConverter.Services
{
    public interface IDateProvider
    {
        DateTime GetLastBusinessDayDate();
    }
}