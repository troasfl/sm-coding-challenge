namespace sm_coding_challenge.Models
{
    public class PlayerModel
    {
        public string Id { get; set; }
        public string EntryId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public RushingModel Rushing { get; set; }
        public KickingModel Kicking { get; set; }
        public PassingModel Passing { get; set; }
        public ReceivingModel Receiving { get; set; }

        public PlayerModel()
        {
            
        }

        public PlayerModel(string id, string entryId, string name, string position)
        {
            Id = id;
            EntryId = entryId;
            Name = name;
            Position = position;
        }
    }
}