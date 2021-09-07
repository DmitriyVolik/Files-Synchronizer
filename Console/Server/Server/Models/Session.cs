using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Session
    {
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set;}

        public int UserId { get; set;}

        [Required]
        public User User { get; set; }
    }
}