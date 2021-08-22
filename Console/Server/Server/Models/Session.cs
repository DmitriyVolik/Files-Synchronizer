using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Session
    {
        public int Id { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string Token { get; set;}

        [Required(AllowEmptyStrings = false)]
        public User User { get; set; }
    }
}