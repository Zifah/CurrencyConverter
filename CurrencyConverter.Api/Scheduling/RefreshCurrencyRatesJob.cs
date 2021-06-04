using CurrencyConverter.Api.Configuration;
using CurrencyConverter.Api.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation;

namespace CurrencyConverter.Api.Scheduling
{
    public class RefreshCurrencyRatesJob : BackgroundService
    {
        private readonly CrontabSchedule schedule;
        private DateTime nextRun;
        private readonly RatesDataRefresher ratesDataRefresher;

        public RefreshCurrencyRatesJob(IOptions<GeneralOptions> generalOptions, RatesDataRefresher ratesDataRefresher)
        {
            this.ratesDataRefresher = Requires.NotNull(ratesDataRefresher, nameof(ratesDataRefresher));

            generalOptions = Requires.NotNull(generalOptions, nameof(generalOptions));
            schedule = CrontabSchedule.Parse(generalOptions.Value.RefreshRatesCron, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            nextRun = schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                var now = DateTime.Now;
                if (now > nextRun)
                {
                    await Process();
                    nextRun = schedule.GetNextOccurrence(DateTime.Now);
                }
                await Task.Delay(60000, stoppingToken); //1 minute delay
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private async Task Process()
        {
            await ratesDataRefresher.RefreshConversionRatesAsync();
        }
    }
}
