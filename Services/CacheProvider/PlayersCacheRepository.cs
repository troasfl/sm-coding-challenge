using System;
using System.Threading.Tasks;
using CacheManager.Core;
using Microsoft.Extensions.Logging;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public class PlayersCacheRepository : IPlayersCacheRepository
    {
        private readonly BaseCacheManager<PlayerModel> _playersCacheManager;
        private readonly ILogger<PlayersCacheRepository> _logger;

        public PlayersCacheRepository(BaseCacheManager<PlayerModel> playersCacheManager, ILogger<PlayersCacheRepository> logger)
        {
            _playersCacheManager = playersCacheManager;
            _logger = logger;
        }

        public Task<PlayerModel> Get(string key)
        {
            try
            {
                return Task.FromResult(_playersCacheManager.Get<PlayerModel>(key));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }

            return null;
        }

        public void Put(string key, PlayerModel playerModel)
        {
            try
            {
                _playersCacheManager.Put(key, playerModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());
            }
        }
    }
}