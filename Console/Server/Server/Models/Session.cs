namespace Server.Models
{
    public class Session
    {
        public int Id { get; set; }

        public string Token { get; set;}

        public User User { get; set; }
    }
}