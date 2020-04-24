using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public class DataProviderImpl : IDataProvider
    {
        private readonly string _key;
        private readonly ILogger<DataProviderImpl> _logger;
        private readonly IPlayersCacheRepository _playersCacheRepository;

        public DataProviderImpl(ILogger<DataProviderImpl> logger, IPlayersCacheRepository playersCacheRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _playersCacheRepository = playersCacheRepository;
            _key = configuration["CacheKey"];
        }

        public async Task<PlayerModel> GetPlayerById(string id)
        {
            try
            {
                return await _playersCacheRepository.Get(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }

            return null;
        }

        public async Task<PlayerModel[]> GetPlayersById(string[] idList)
        {
            try
            {
                var tasks = new List<Task<PlayerModel>>();

                // cache data for each player for rushing stats
                foreach (var player in idList)
                    tasks.Add(GetPlayerById(player));
                return await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }

            return null;
        }

        public async Task<LatestPlayers> GetLatestPlayersById(string[] idList)
        {
            await Task.Delay(0);

            try
            {
                var r = CommonCachingService.RawDataCache.Get<DataResponseModel>(_key);
                var result = new LatestPlayers
                {
                    Receiving = r.Receiving,
                    Rushing = r.Rushing,
                    Passing = r.Passing,
                    Kicking = r.Kicking
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }

            return null;
        }
    }
}