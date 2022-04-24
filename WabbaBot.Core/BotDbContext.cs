using Microsoft.EntityFrameworkCore;
using WabbaBot.Models;

namespace WabbaBot.Core {
    public partial class BotDbContext : DbContext {
        #region Properties
        public DbSet<Maintainer> Maintainers { get; set; }
        public DbSet<ManagedModlist> ManagedModlists { get; set; }
        public DbSet<SubscribedChannel> SubscribedChannels { get; set; }
        public DbSet<PingRole> PingRoles { get; set; }
        public string DbPath { get; }
        #endregion

        public BotDbContext() {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "WabbaBot.db");
        }

        #region Methods
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.Maintainers)
                        .WithMany(maintainer => maintainer.ManagedModlists);

            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.SubscribedChannels)
                        .WithMany(maintainer => maintainer.ManagedModlists);

            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.PingRoles)
                        .WithOne(pingRole => pingRole.ManagedModlist);
        }
        #endregion
    }
}
