using System.Threading.Tasks;
using CacheManager.Core;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public class PlayersCacheRepository : IPlayersCacheRepository
    {
        private readonly BaseCacheManager<PlayerModel> _playersCacheManager;

        public PlayersCacheRepository(BaseCacheManager<PlayerModel> playersCacheManager)
        {
            _playersCacheManager = playersCacheManager;
        }

        public Task<PlayerModel> Get(string key)
        {
            return Task.FromResult(_playersCacheManager.Get<PlayerModel>(key));
        }

        public void Put(string key, PlayerModel playerModel)
        {
            _playersCacheManager.Put(key, playerModel);
        }
    }
}