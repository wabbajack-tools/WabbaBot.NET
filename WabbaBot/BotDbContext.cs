﻿using Microsoft.EntityFrameworkCore;
using WabbaBot.Models;

namespace WabbaBot {
    public partial class BotDbContext : DbContext {
        #region Properties
        public DbSet<Maintainer> Maintainers { get; set; }
        public DbSet<ManagedModlist> ManagedModlists { get; set; }
        public DbSet<SubscribedChannel> SubscribedChannels { get; set; }
        public DbSet<PingRole> PingRoles { get; set; }
        public DbSet<ReleaseMessage> ReleaseMessages { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<ReleaseTemplate> ReleaseTemplates { get; set; }

        public string DbPath { get; }
        #endregion

        public BotDbContext() {
            var path = Directory.GetCurrentDirectory();
            DbPath = Path.Join(path, "WabbaBot.db");
        }

        #region Methods
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // ManagedModlist
            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.Maintainers)
                        .WithMany(maintainer => maintainer.ManagedModlists);

            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.SubscribedChannels)
                        .WithMany(maintainer => maintainer.ManagedModlists);

            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.PingRoles)
                        .WithOne(pingRole => pingRole.ManagedModlist)
                        .HasForeignKey(pingRole => pingRole.ManagedModlistId);

            modelBuilder.Entity<ManagedModlist>()
                        .HasMany(modlist => modlist.ReleaseMessages)
                        .WithOne(releaseMessage => releaseMessage.ManagedModlist)
                        .HasForeignKey(releaseMessage => releaseMessage.ManagedModlistId);

            modelBuilder.Entity<ReleaseMessage>()
                        .HasOne(releaseMessage => releaseMessage.SubscribedChannel)
                        .WithMany(subscribedChannel => subscribedChannel.ReleaseMessages)
                        .HasForeignKey(releaseMessage => releaseMessage.SubscribedChannelId);

            modelBuilder.Entity<ReleaseMessage>()
                        .HasOne(releaseMessage => releaseMessage.Maintainer)
                        .WithMany(maintainer => maintainer.ReleaseMessages)
                        .HasForeignKey(releaseMessage => releaseMessage.MaintainerId);

            modelBuilder.Entity<ReleaseMessage>()
                        .HasOne(releaseMessage => releaseMessage.ManagedModlist)
                        .WithMany(managedModlist => managedModlist.ReleaseMessages)
                        .HasForeignKey(releaseMessage => releaseMessage.ManagedModlistId);

            modelBuilder.Entity<ReleaseMessage>()
                        .HasOne(releaseMessage => releaseMessage.Release)
                        .WithMany(releaseMessageGroup => releaseMessageGroup.ReleaseMessages)
                        .HasForeignKey(releaseMessage => releaseMessage.ReleaseId);

            modelBuilder.Entity<ReleaseTemplate>()
                        .HasOne(releaseTemplate => releaseTemplate.ManagedModlist)
                        .WithOne(managedModlist => managedModlist.ReleaseTemplate)
                        .HasForeignKey<ReleaseTemplate>(releaseTemplate => releaseTemplate.ManagedModlistId);
        }

        public override int SaveChanges() {
            var entries = ChangeTracker.Entries();

            var models = entries.Where(x => x.Entity != null && x.Entity is ABaseModel);
            var newModels = models.Where(x => x.State == EntityState.Added).Select(x => (ABaseModel)x.Entity);

            foreach (var newModel in newModels) {
                newModel.CreatedOn = DateTime.UtcNow;
                newModel.ModifiedOn = DateTime.UtcNow;
            }

            var modifiedModels = models.Where(x => x.State == EntityState.Modified).Select(x => (ABaseModel)x.Entity);
            foreach (var modifiedModel in modifiedModels) {
                modifiedModel.ModifiedOn = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }
        #endregion
    }
}
