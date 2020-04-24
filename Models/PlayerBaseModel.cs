using System.Runtime.Serialization;

namespace sm_coding_challenge.Models
{
    [DataContract]
    public class PlayerBaseModel
    {
        [DataMember(Name = "player_id")]
        public string Id { get; set; }
        
        [DataMember(Name = "entry_id")]
        public string EntryId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "position")]
        public string Position { get; set; }
    }
}

