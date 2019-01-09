using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EntrepreneurCommon.DbLayer;
using EntrepreneurCommon.Models;
using EntrepreneurCommon.Models.Central;
using EntrepreneurCommon.Models.DatabaseModels;
using EntrepreneurCommon.Models.EsiResponseModels;

namespace EveEntrepreneurWebPersistency3.Models
{
    public class AppDbContext : DbContext, IDbContextFactory<AppDbContext>
    {
        public AppDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        public AppDbContext() : base() { }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        // Application Database
        /// <summary>
        /// Storage for EsiTokens
        /// </summary>
        public DbSet<DbTokenWrapper> Tokens { get; set; }
        /// <summary>
        /// Cached Id to Name values
        /// </summary>
        public DbSet<NameCache> NameCache { get; set; }
        /// <summary>
        /// Cache timers for endpoint calls
        /// </summary>
        public DbSet<CacheTimer> CacheTimers { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        
        // Public Information Entries
        public DbSet<CharacterPublicInformation> CharacterPublicInformation { get; set; }
        public DbSet<CorporationPublicInformation> CorporationInformation { get; set; }



        public AppDbContext Create()
        {
            throw new NotImplementedException();
        }

        // Persistent Database Entries
        public DbSet<CharacterMarketOrdersHistoryModel> CharacterMarketOrdersHistory { get; set; }
        public DbSet<CharacterMiningLedgerModel> CharacterMiningLedger { get; set; }
        public DbSet<CharacterWalletJournalModel> CharacterWalletJournal { get; set; }
        public DbSet<CharacterWalletTransactionModel> CharacterWalletTransactions { get; set; }
        public DbSet<CorporationMarketOrdersHistory> CorporationMarketOrdersHistory { get; set; }
        public DbSet<CorporationMiningExtractionModel> CorporationMiningExtractions { get; set; }
        public DbSet<CorporationMiningObserverLedgerModel> CorporationMiningLedger { get; set; }
        public DbSet<CorporationMiningObserversModel> CorporationMiningObservers { get; set; }
        public DbSet<CorporationWalletJournalModel> CorporationWalletJournal { get; set; }
        public DbSet<CorporationWalletTransactionModel> CorporationWalletTransactions { get; set; }
        public DbSet<MarketsRegionHistoryResponse> MarketsRegionHistory { get; set; }
    }
}