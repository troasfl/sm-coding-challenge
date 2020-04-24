using System.Runtime.Serialization;

namespace sm_coding_challenge.Models
{
    public class PlayerBasePassingStatsModel : PlayerBaseModel
    {
        [DataMember(Name = "yds")]
        public string Yds { get; set; } 
        
        [DataMember(Name = "att")]
        public string Att { get; set; } 
        
        [DataMember(Name = "tds")]
        public string Tds { get; set; } 
        
        [DataMember(Name = "cmp")]
        public string Cmp { get; set; } 
        
        [DataMember(Name = "int")]
        public string Int { get; set; }
    }
}