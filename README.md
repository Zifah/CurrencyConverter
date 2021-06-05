# Introduction
An Exchange Rates and Currency Conversion service. The service provides the following endpoints:
- **GET "/exchangerates"**: Provides both current and historical exchange rates between two currencies
- **POST "/currencyConverter"**: Converts an amount from one currency to another using the current exchange rate. Also does historical conversions

Further information about the endpoints can be found in the Swagger documentation of the service

# How to run the service
1. Clone the repo to your machine
2. Before launch, ensure that your machine has network access to the internet address "https://www.ecb.europa.eu". The exchange rates are loaded in from that address into memory as part of application startup
3. Launch the solution through Visual Studio IIS Express or self-hosted mode. The service launchUrl is set to "/swagger", so the Swagger documentation should be visible after start-up
4. Make a test request using the Swagger doc

# Other things to know
1. All the rates are held in memory, so there is no need to set up an external database to run the service