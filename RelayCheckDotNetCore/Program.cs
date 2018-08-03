using Microsoft.Azure.Relay;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RelayCheckDotNetCore
{
    class Program
    {
        private const string RelayNamespace = "xxx.servicebus.windows.net";
        private const string ConnectionName = "devhttpcon";
        private const string KeyName = "devkey";
        private const string Key = "xxx";

        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
             KeyName, Key);
            var uri = new Uri(string.Format("https://{0}/{1}", RelayNamespace, ConnectionName));
            var token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;

            JObject payLoad = new JObject(
               new JProperty("username", "Username"),
               new JProperty("password", "User Password"),
               new JProperty("token", "xxxxxx")
           );
            var httpContent = new StringContent(payLoad.ToString(), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = httpContent
            };
            request.Headers.Add("ServiceBusAuthorization", token);
            var response = await client.SendAsync(request);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
