using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var builder= new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();
            optionsBuilder.UseNpgsql(config["Database"]);
        }

    }
}