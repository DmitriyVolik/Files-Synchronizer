using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Group
    {
        public int Id { get; set; }
        
        [MaxLength(16)]
        public string Name { get; set; }

        public string FolderPath { get; set; }


    }
}