using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Covid_parser
{
    class Program
    {
        private const string data_url = @"https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv";
        static void Main(string[] args)
        {
            bool b;
            string country = null;
            do
            {
                b = false;
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("Write 'EXIT' to close the program");
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("Write 'INFO' to show information");
                Console.WriteLine(new string('-', 50));
                Console.Write("Input country: ");
                country = Console.ReadLine();

                switch (country)
                {
                    case "EXIT":
                        break;
                    case "INFO":
                        ShowInfo();
                        b = true;
                        break;
                    default:
                        b = true;
                        try
                        {
                            ShowData(country);
                        }
                        catch
                        {
                            Console.WriteLine("Incorrect input! Try again or input INFO");
                        }
                        break;
                }
            }
            while (b);
        }
        private static void ShowInfo()
        {
            var list = new HashSet<string>();
            var lines = GetDataLines()
                .Skip(1)
                .Select(line => line.Split(','));
            foreach (var row in lines)
            {
                var country_name = row[1].Trim(' ', '"');
                list.Add(country_name);
                       
                //Console.WriteLine(country_name);

            }
            foreach(var item in list)
            {
                Console.WriteLine(item);
            }


            //Console.WriteLine(string.Join("\r\n", );
        }

        private static void ShowData(string country)
        {
            var data = GetData()
                        .First(v => v.country_name.Equals(country, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine(string.Join("\r\n", GetDateTimes().Zip(data.counts, (date, count) => $"Date:  {date:dd:MM:yyyy} - Cases:  { count}")));
        }


        private static IEnumerable<(string country_name, string province, int [] counts)> GetData()
        {

            var lines = GetDataLines()
                .Skip(1)
                .Select(line => line.Split(','));
            foreach(var row in lines)
            {
                var province = row[0].Trim();
                var country_name = row[1].Trim(' ','"');
                var counts = row.Skip(4).Select(int.Parse).ToArray();

                yield return (country_name, province, counts);
            }

        }

        private static IEnumerable<string> GetDataLines()
        {
            using (var data_stream = GetDataStreamAsync().Result)
            using (var data_reader = new StreamReader(data_stream))

                while (!data_reader.EndOfStream)
                {
                    var line = data_reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    yield return line.Replace("Korea,", "Korea -");
                }

        }

        private static async Task<Stream> GetDataStreamAsync()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(data_url, HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync();
        }

        private static DateTime[] GetDateTimes() => GetDataLines()
            .First()
            .Split(',')
            .Skip(4)
            .Select(s => DateTime.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
    }
}
