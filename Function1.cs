using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace functionWithTwilio
{
    public class Function1
    {
        [FunctionName("Function1")]
        [return: TwilioSms(
            AccountSidSetting = "TwilioAccountSid",
            AuthTokenSetting = "TwilioAuthToken"
        )]
        public async Task<CreateMessageOptions> RunAsync([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"SendSmsTimer executed at: {DateTime.Now}");

            string toPhoneNumber = Environment.GetEnvironmentVariable("ToPhoneNumber", EnvironmentVariableTarget.Process);
            string fromPhoneNumber = Environment.GetEnvironmentVariable("FromPhoneNumber", EnvironmentVariableTarget.Process);
            string apiKeydemo = Environment.GetEnvironmentVariable("apiKey", EnvironmentVariableTarget.Process);

            var message = new CreateMessageOptions(new PhoneNumber(toPhoneNumber));
            log.LogInformation("C# HTTP trigger function processed a request.");



            //string stock = req.Query["stock"];

            //string condition = req.Query["condition"];

            //double threshold = Convert.ToDouble(req.Query["threshold"]);

            string stockName = "Zomato.BSE";

            string requestUrl = $"https://www.alphavantage.co/query?function=TIME_SERIES_WEEKLY&symbol="+stockName+"&apikey="+apiKeydemo;

            var httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDocument = JsonDocument.Parse(responseData);

                JsonElement weeklyTimeSeries = jsonDocument.RootElement.GetProperty("Weekly Time Series");
                JsonElement mostRecentWeek = weeklyTimeSeries.EnumerateObject().First().Value;
                string openPrice = mostRecentWeek.GetProperty("1. open").GetString();

               
                message = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
                {
                    From = new PhoneNumber(fromPhoneNumber),
                    Body = $"Hello from DJ's most recent Application, the stock price for {stockName} today is: {openPrice}"
                };

            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                message = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
                {
                    From = new PhoneNumber(fromPhoneNumber),
                    Body = $"Hello from DJ's most recent Application, there's something wrong with your app"
                };
            }
            
           
            return message;
        }
    }
}
