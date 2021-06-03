using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CurrencyConverter.Api.Helpers
{
    public class EuroFxCsvHelper
    {
        private const string DateColumnName = "Date";
        public IDictionary<DateTime, IDictionary<string, decimal?>> ParseText(string text)
        {
            return ParseText(new StringReader(text));
        }

        public IDictionary<DateTime, IDictionary<string, decimal?>> ParseText(TextReader reader)
        {
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();

            var result = new Dictionary<DateTime, IDictionary<string, decimal?>>();
            while (csv.Read())
            {
                var dictionary = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                var rowDate = Convert.ToDateTime(dictionary[DateColumnName]);
                dictionary.Remove(DateColumnName);

                result.Add(rowDate, dictionary.ToDictionary(kvp => kvp.Key, kvp => {
                    if(decimal.TryParse(kvp.Value.ToString(), out decimal rate)){
                        return (decimal?)rate;
                    }
                    return null;
                }));
            }
            return result;
        }
    }
}
