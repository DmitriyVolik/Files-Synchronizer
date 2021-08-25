using System;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Group
    {
        public int Id { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        [MaxLength(16)]
        public string Name { get; set; }

        [Required]
        public string FolderPath { get; set; }
        
        
    }
}