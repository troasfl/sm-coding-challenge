using System.Collections.Generic;

namespace sm_coding_challenge.Models
{
    public class LatestPlayers
    {
        public List<PlayerBaseKickingStatsModel> Kicking { get; set; }
        public List<PlayerBaseRushingStatsModel> Rushing { get; set; }
        public List<PlayerBasePassingStatsModel> Passing { get; set; }
        public List<PlayerBaseReceivingStatsModel> Receiving { get; set; }
        
    }
}