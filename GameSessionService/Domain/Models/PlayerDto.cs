namespace GameSession.Domain.Models
{
    public record PlayerDto
    {
        public Guid Id { get; }
        public string UserName { get; }
        public int Lvl { get; }

        public PlayerDto(Guid id, string userName, int lvl)
        {
            Id = id;
            UserName = userName;
            Lvl = lvl;
        }
    }
}
