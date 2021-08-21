using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [MaxLength(16)]
        public string Login { get; set; }
        
        [MaxLength(100)]
        public string Password { get; set; }

        public Group Group { get; set; }
        
    }
}