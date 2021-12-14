using Microsoft.EntityFrameworkCore;
using ServerChat.Models;

namespace ServerChat
{
    public sealed class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();
        }

        public DbSet<User> Users { get; set; } 
        public DbSet<UserOnline> UsersOnline { get; set; } 
    }
}