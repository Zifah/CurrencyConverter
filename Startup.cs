using CurrencyConverter.Configuration;
using CurrencyConverter.Helpers;
using CurrencyConverter.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            services.AddSingleton<IRatesDataStore, InMemoryDictionaryRatesDataStore>();
            services.AddScoped<EuroFxCsvHelper>();
            services.AddScoped<IThirdPartyRatesDataSource, EuroFxRefRatesDataSource>();
            services.AddScoped<ICurrencyDataProvider, EuroFxRefRatesProvider>();
            services.AddScoped<IFileOperations, FileOperations>();
            services.AddScoped<IDateProvider>((serviceProvider) => new DateProvider(DateTime.Now, serviceProvider.GetService<IOptions<GeneralOptions>>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ICurrencyDataProvider currencyDataProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Refresh conversion rates on startup
            currencyDataProvider.RefreshConversionRatesAsync();
        }
    }
}
