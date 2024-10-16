namespace Match.Domain.Models
{
    public struct Player
    {
        public Guid Id { get; }
        public string UserName { get; }
        public int Lvl { get; }

        public Player(Guid id, string userName, int lvl)
        {
            Id = id;
            UserName = userName;
            Lvl = lvl;
        }
    }
}
