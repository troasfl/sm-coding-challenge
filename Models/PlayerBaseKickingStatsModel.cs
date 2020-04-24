using System.Runtime.Serialization;

namespace sm_coding_challenge.Models
{
    public class PlayerBaseKickingStatsModel : PlayerBaseModel
    {
        [DataMember(Name = "fld_goals_made")]
        public string FldGoalsMade { get; set; }

        [DataMember(Name = "fld_goals_att")]
        public string FldGoalsAtt { get; set; }

        [DataMember(Name = "extra_pt_made")]
        public string ExtraPtMade { get; set; }

        [DataMember(Name = "extra_pt_att")]
        public string ExtraPtAtt { get; set; }
    }
}