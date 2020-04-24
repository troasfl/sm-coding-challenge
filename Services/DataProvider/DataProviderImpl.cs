using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Return a single player 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PlayerModel> GetPlayerById(string id)
        {
            return await _playersCacheRepository.Get(id);
        }


        /// <summary>
        /// Returns an array of players
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public async Task<PlayerModel[]> GetPlayersById(string[] idList)
        {
            var tasks = new List<Task<PlayerModel>>();

            foreach (var player in idList)
                tasks.Add(GetPlayerById(player));
            var result = await Task.WhenAll(tasks);
            return result.Where(model => model != null).ToArray();
        }


        /// <summary>
        /// Returns the latest players for each category (kicking, passing, receiving and rushing)
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public async Task<LatestPlayers> GetLatestPlayersById(string[] idList)
        {
            await Task.Delay(0);

            try
            {
                var dataResponseModel = CommonCachingService.RawDataCache.Get<DataResponseModel>(_key);
                if (dataResponseModel != null)
                {
                    var result = new LatestPlayers
                    {
                        Receiving = dataResponseModel.Receiving,
                        Rushing = dataResponseModel.Rushing,
                        Passing = dataResponseModel.Passing,
                        Kicking = dataResponseModel.Kicking
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }

            return null;
        }
    }
}