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
        [MinLength(4)]
        [Required]
        public string Login { get; set; }
        
        [MaxLength(100)]
        [Required]
        [MinLength(4)]
        public string Password { get; set; }
        public Group Group { get; set; }

    }
}