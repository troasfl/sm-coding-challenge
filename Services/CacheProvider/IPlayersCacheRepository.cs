using System.Threading.Tasks;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public interface IPlayersCacheRepository
    {

        public Task<PlayerModel> Get(string key);
        void Put(string key, PlayerModel playerModel);
    }
}