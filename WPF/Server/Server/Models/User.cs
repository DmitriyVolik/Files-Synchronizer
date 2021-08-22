using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [MaxLength(16)]
        [Required(AllowEmptyStrings = false)]
        public string Login { get; set; }
        
        [MaxLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
        
        public Group Group { get; set; }

    }
}