using Microsoft.Azure.Relay;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerDotNetCore
{
    public class Program
    {
        private const string RelayNamespace = "xxx.servicebus.windows.net";
        private const string ConnectionName = "devhttpcon";
        private const string KeyName = "devkey";
        private const string Key = "xxx";

        public static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, Key);
            var listener = new HybridConnectionListener(new Uri(string.Format("sb://{0}/{1}", RelayNamespace, ConnectionName)), tokenProvider);

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Online"); };

          
            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {

                // Do something with context.Request.Url, HttpMethod, Headers, InputStream...
                if (context.Request.HasEntityBody)
                {
                    Console.WriteLine(context.Request.InputStream.ReadByte().ToString());

                    System.IO.Stream body = context.Request.InputStream;
                    System.IO.StreamReader reader = new System.IO.StreamReader(body, Encoding.UTF8);

                    Console.WriteLine("Start of client data:");
                    // Convert the data to a string and display it on the console.
                    string s = reader.ReadToEnd();
                    Console.WriteLine(s);
                    Console.WriteLine("End of client data:");

                }
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.StatusDescription = "OK";
                using (var sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.WriteLine("hello!");
                }

                // The context MUST be closed here
                context.Response.Close();
            };

            // Opening the listener establishes the control channel to
            // the Azure Relay service. The control channel is continuously 
            // maintained, and is reestablished when connectivity is disrupted.
            await listener.OpenAsync();
            Console.WriteLine("Server listening");

            // Start a new thread that will continuously read the console.
            await Console.In.ReadLineAsync();

            // Close the listener after you exit the processing loop.
            await listener.CloseAsync();
        }

        public static void ShowRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                Console.WriteLine("No client data was sent with the request.");
                return;
            }
            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                Console.WriteLine("Client data content type {0}", request.ContentType);
            }
            Console.WriteLine("Client data content length {0}", request.ContentLength64);

            Console.WriteLine("Start of client data:");
            // Convert the data to a string and display it on the console.
            string s = reader.ReadToEnd();
            Console.WriteLine(s);
            Console.WriteLine("End of client data:");
            body.Close();
            reader.Close();
            // If you are finished with the request, it should be closed also.
        }
    }
}
