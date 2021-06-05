using CurrencyConverter.Api.Configuration;
using CurrencyConverter.Api.Exceptions;
using CurrencyConverter.Api.Helpers;
using CurrencyConverter.Api.Scheduling;
using CurrencyConverter.Api.Services;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace CurrencyConverter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.Configure<EuroFxRefOptions>(Configuration.GetSection("EuroFxRef"));
            services.Configure<GeneralOptions>(Configuration.GetSection("General"));
            services.AddHttpClient(ApplicationConstants.EuroFxRefNamedClientName, c =>
            {
                string baseAddress = Configuration.GetValue<string>("EuroFxRef:BaseUrl");
                c.BaseAddress = new Uri(baseAddress);
            });

            services.AddScoped<ICurrencyDataProvider, EuroFxRefRatesProvider>();
            services.AddTransient<EuroFxCsvHelper>();
            services.AddTransient<IFileOperations, FileOperations>();
            services.AddTransient<IThirdPartyRatesDataSource, EuroFxRefRatesDataSource>();
            services.AddTransient<IDateProvider>((serviceProvider) => new DateProvider(DateTime.Now, serviceProvider.GetService<IOptions<GeneralOptions>>()));
            services.AddTransient<RatesDataRefresher>();
            services.AddSingleton<IRatesDataStore, InMemoryDictionaryRatesDataStore>();

            services.AddHostedService<RefreshCurrencyRatesJob>();

            services.AddProblemDetails(opts =>
            {
                opts.IncludeExceptionDetails = (context, ex) =>
                {
                    var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
                    return environment.IsDevelopment();
                };

                opts.Map<RateNotFoundException>(exception => new StatusCodeProblemDetails(StatusCodes.Status404NotFound)
                {
                    Detail = exception.Message
                });
            });

            // Register the Swagger services
            services.AddSwaggerDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RatesDataRefresher ratesDataRefresher)
        {
            app.UseProblemDetails();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Refresh conversion rates on startup
            ratesDataRefresher.RefreshConversionRatesAsync().Wait();
        }
    }
}
