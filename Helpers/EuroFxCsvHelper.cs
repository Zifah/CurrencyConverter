using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CurrencyConverter.Helpers
{
    public class EuroFxCsvHelper
    {
        private const string DateColumnName = "Date";
        public IDictionary<DateTime, IDictionary<string, string>> ParseText(string text)
        {
            return ParseText(new StringReader(text));
        }

        public IDictionary<DateTime, IDictionary<string, string>> ParseText(TextReader reader)
        {
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            csv.Context.RegisterClassMap(new DictionaryClassMap(csv.HeaderRecord));

            var result = new Dictionary<DateTime, IDictionary<string, string>>();
            while (csv.Read())
            {
                var dictionary = csv.GetRecord<dynamic>() as IDictionary<string, string>;
                var rowDate = Convert.ToDateTime(dictionary[DateColumnName]);
                dictionary.Remove(DateColumnName);
                result.Add(rowDate, dictionary);
            }
            return result;
        }

        private class DictionaryClassMap : ClassMap<Dictionary<string, string>>
        {
            private readonly IEnumerable<string> _headers;

            public DictionaryClassMap(IEnumerable<string> headers)
            {
                _headers = headers;
                foreach (var header in _headers)
                {
                    Map(x => x[header]);
                }
            }
        }
    }
}
