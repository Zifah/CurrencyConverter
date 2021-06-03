using CurrencyConverter.Api.Configuration;
using CurrencyConverter.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyConverter.UnitTests.Services
{
    [TestFixture]
    public class DateProviderTests
    {
        private const string Monday = "31 May 2021";
        private const string Tuesday = "1 Jun  2021";
        private const string Wednesday = "2 Jun  2021";
        private const string Thursday = "3 Jun  2021";
        private const string Friday = "4 Jun  2021";
        private const string BusinessDayStart = "16:00";
        private DateTime nowTime = new DateTime();
        private IOptions<GeneralOptions> generalOptions;
        private DateProvider instance;

        [SetUp]
        public void SetUp()
        {
            generalOptions = Options.Create(new GeneralOptions
            {
                BusinessDayStart = BusinessDayStart
            });

            instance = new DateProvider(nowTime, generalOptions);
        }

        [TestCase(Monday)] // Monday
        [TestCase(Tuesday)] // Tuesday
        [TestCase(Wednesday)] // Wednesday
        [TestCase(Thursday)] // Thursday
        [TestCase(Friday)] // Friday
        public void GetCurrentBusinessDayDate_ReturnsSameDate_WhenInputDateIsAWeekDay(string date)
        {
            // Arrange
            var expectedBusinessDate = Convert.ToDateTime(date);

            // Act
            var actualBusinessDate = instance.GetCurrentBusinessDayDate(expectedBusinessDate);

            // Assert
            actualBusinessDate.Should().BeSameDateAs(expectedBusinessDate);
        }

        [TestCase("5 Jun 2021", "4 Jun 2021")] // Saturday, the last business day is one day before
        [TestCase("6 Jun 2021", "4 Jun 2021")] // Sunday, the last business day is two days before
        public void GetCurrentBusinessDayDate_ReturnsLastBusinessDay_WhenInputDateIsAWeekend(string weekendDate, string lastBusinessDate)
        {
            // Arrange
            var inputWeekendDate = Convert.ToDateTime(weekendDate);
            var expectedLastBusinessDate = Convert.ToDateTime(lastBusinessDate);

            // Act
            var actualLastBusinessDay = instance.GetCurrentBusinessDayDate(inputWeekendDate);

            // Assert
            actualLastBusinessDay.Should().BeSameDateAs(expectedLastBusinessDate);
        }

        [TestCase("5 Jun 2021", "4 Jun 2021")] // Saturday, last working day is Friday
        [TestCase("6 Jun 2021", "4 Jun 2021")] // Sunday, last working day is Friday
        public void GetCurrentBusinessDayDate_ReturnsCurrentBusinessDay_WhenNowDateIsAWeekend_AndAsAtDateIsNotSpecified(string nowDate, string expectedBusinessDate)
        {
            // Arrange
            instance = new DateProvider(Convert.ToDateTime(nowDate), generalOptions);

            // Act
            var actualLastBusinessDay = instance.GetCurrentBusinessDayDate();

            // Assert
            actualLastBusinessDay.Should().BeSameDateAs(Convert.ToDateTime(expectedBusinessDate));
        }

        public void GetCurrentBusinessDayDate_ReturnsPreviousBusinessDayDate_WhenCurrentDayIsBusinessDay_ButTimeEarlierThanBusinessStartTime()
        {
            // Arrange
            string aFridayAt1559 = "4 Jun 2021 15:59";
            nowTime = Convert.ToDateTime(aFridayAt1559);
            SetUp();
            var oneDayBefore = Convert.ToDateTime("3 Jun 2021");

            // Act
            var actualLastBusinessDay = instance.GetCurrentBusinessDayDate();

            // Assert
            actualLastBusinessDay.Should().BeSameDateAs(oneDayBefore);
        }

        [TestCase("3 Jun 2021 16:00")]
        [TestCase("3 Jun 2021 16:01")]
        [TestCase("3 Jun 2021 23:59")]
        [TestCase("4 Jun 2021 16:00")]
        [TestCase("4 Jun 2021 16:01")]
        [TestCase("4 Jun 2021 23:59")]
        public void GetCurrentBusinessDayDate_ReturnsCurrentDate_WhenCurrentDayIsBusinessDay_AndBusinessStartTimeIsReached(string nowTimeString)
        {
            // Arrange
            nowTime = Convert.ToDateTime(nowTimeString);
            SetUp();
            var currentDate = nowTime.Date;

            // Act
            var actualLastBusinessDay = instance.GetCurrentBusinessDayDate();

            // Assert
            actualLastBusinessDay.Should().BeSameDateAs(currentDate);
        }

        [TestCase(Tuesday, Monday)]
        [TestCase(Wednesday, Tuesday)]
        [TestCase(Thursday, Wednesday)]
        [TestCase(Friday, Thursday)]
        public void GetLastBusinessDayDate_ReturnsPreviousBusinessDayDate(string currentBusinessDay, string previousBusinessDay)
        {
            // Arrange
            nowTime = Convert.ToDateTime(currentBusinessDay);
            var previousBusinessDayDate = Convert.ToDateTime(previousBusinessDay);
            SetUp();

            // Act
            var lastBusinessDayDate = instance.GetLastBusinessDayDate();

            // Assert
            lastBusinessDayDate.Should().BeSameDateAs(previousBusinessDayDate);
        }
    }
}
