using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace sm_coding_challenge.Services.DataProvider
{
    public interface ICustomHttpClient
    {
        HttpClient Client { get; }

        Task<HttpResponseMessage> MakeGetRequestAsync(string url, CancellationToken token, string mediaType = "application/json");
    }
}