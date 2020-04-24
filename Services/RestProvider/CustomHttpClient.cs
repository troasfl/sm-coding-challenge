using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace sm_coding_challenge.Services.DataProvider
{
    public class CustomHttpClient : ICustomHttpClient
    {
        public CustomHttpClient(HttpClient client)
        {
            Client = client;
            Client.Timeout = TimeSpan.FromSeconds(30);
            Client.DefaultRequestHeaders.Clear();
        }

        public HttpClient Client { get; }

        public async Task<HttpResponseMessage> MakeGetRequestAsync(string url, CancellationToken token, string mediaType = "application/json")
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                var response = await Client.SendAsync(httpRequestMessage, token);
                return response;
            }
            catch (OperationCanceledException exception)
            {
                if (token.IsCancellationRequested) throw;
                throw new TimeoutException("The request timed out.", exception);
            }
        }

    }
}