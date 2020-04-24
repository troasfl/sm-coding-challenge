using CacheManager.Core;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public class CommonCachingService
    {
        public static ICacheManager<DataResponseModel> RawDataCache;
    }
}