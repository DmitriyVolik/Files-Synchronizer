using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Database
{
    public class Context:DbContext
    {
        public DbSet<User> Users { get; set; }
        
        public DbSet<Group> Groups { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public Context()
        {
            /*Database.EnsureDeleted();
            Database.EnsureCreated();*/
        }
 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            /*optionsBuilder.UseNpgsql(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);*/

            optionsBuilder.UseNpgsql("Host=localhost;Database=FilesSynchronizer;Username=postgres;Password=database");
        }

    }
}