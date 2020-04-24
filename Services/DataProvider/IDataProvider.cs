using System.Threading.Tasks;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public interface IDataProvider
    {
        Task<PlayerModel> GetPlayerById(string id);
        Task<PlayerModel[]> GetPlayersById(string[] idList);
        Task<LatestPlayers> GetLatestPlayersById(string[] idList);
    }
}
