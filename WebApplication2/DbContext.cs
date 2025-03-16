using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace WebApplication2
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Skin> Skins { get; set; }
        public DbSet<UserSkin> UserSkins { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка связи многие-ко-многим
            modelBuilder.Entity<UserSkin>()
                .HasKey(us => new { us.UserId, us.SkinId });

            modelBuilder.Entity<UserSkin>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkins)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSkin>()
                .HasOne(us => us.Skin)
                .WithMany(s => s.UserSkins)
                .HasForeignKey(us => us.SkinId);
        }
    }
}