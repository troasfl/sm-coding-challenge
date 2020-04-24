using System.Runtime.Serialization;

namespace sm_coding_challenge.Models
{
    public class PlayerBaseReceivingStatsModel : PlayerBaseModel
    {
        [DataMember(Name = "yds")]
        public string Yds { get; set; } 
        
        [DataMember(Name = "tds")]
        public string Tds { get; set; } 
        
        [DataMember(Name = "rec")]
        public string Rec { get; set; }
    }
}