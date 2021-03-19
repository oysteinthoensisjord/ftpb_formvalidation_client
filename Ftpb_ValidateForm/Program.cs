using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ftpb_ValidateForm
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            var appSettings = serviceProvider.GetService<IOptions<AppSettings>>();

            var sourceDirectory = appSettings.Value.ValidationFileDirectory;
            var filename = "vorpa-bp-17500.xml";
            var validationFile = Path.Combine(sourceDirectory, filename);

            Console.WriteLine($"Validating {validationFile}");

            var validationContent = new ValidateFormv2();
            validationContent.DataFormatId = "6954";
            validationContent.DataFormatVersion = "46381";
            validationContent.FormData = File.ReadAllText(validationFile);
            validationContent.AttachmentTypesAndForms = new ValidateAttachment[0];

            var payload = JsonConvert.SerializeObject(validationContent);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(appSettings.Value.ValidationAPIHost);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                client.Timeout = new TimeSpan(0, 5, 0);
                var basicAuth = Encoding.UTF8.GetBytes(appSettings.Value.BasicAuth);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(basicAuth));
                client.DefaultRequestHeaders.Add("accept", "application/json");
                var stopWatch = Stopwatch.StartNew();

                var result = await client.PostAsync("api/validatev2/form", content);

                result.Headers.TryGetValues("x-correlation-id", out IEnumerable<string> header);
                Console.WriteLine($"x-correlation-id - {header?.FirstOrDefault()}");

                var contentString = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                    Console.WriteLine(contentString);


                Console.WriteLine($"Elapsed time (ms): {stopWatch.ElapsedMilliseconds}");
                File.WriteAllText($"vorpa-validation-result.json", contentString);
                Console.ReadLine();
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var environmentName = Environment.GetEnvironmentVariable("ENVIRONMENT");

            // create service collection
            var services = new ServiceCollection();

            // build config
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            // setup config
            services.AddOptions();
            services.Configure<AppSettings>(configuration.GetSection("App"));

            // create service provider
            return services.BuildServiceProvider();
        }
    }
}
