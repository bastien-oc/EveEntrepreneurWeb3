using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Common.Attributes;
using EntrepreneurCommon.ExtensionMethods;
using EntrepreneurCommon.Helpers;
using EntrepreneurCommon.Models.EsiResponseModels;
using EveEntrepreneurWebPersistency3.Extensions;
using EveEntrepreneurWebPersistency3.Models;
using Newtonsoft.Json;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Logic
{
    public delegate void UpdateJob(int id);

    public enum UpdateWorkerType
    {
        CharacterWalletJournal,
        CharacterWalletTransactions,
        CharacterMarketOrdersHistory,
        CharacterMiningJournal,
        CorporationWalletJournal,
        CorporationWalletTransactions,
        CorporationMarketOrdersHistory,
        CorporationMiningDone
    }

    public static class JobHelper
    {
        public static List<UpdateWorker> UpdateWorkers = new List<UpdateWorker>();

        /// <summary>
        ///     Creates and stores a cache timer derived from the provided response object.
        /// </summary>
        /// <param name="response"></param>
        public static async Task RegisterCacheTimer(IRestResponse response)
        {
            var context = new AppDbContext();
            var timer   = CacheLogic.CreateCacheTimer(response);
            context.CacheTimers.AddOrUpdate(timer);
            await context.SaveChangesAsync();
        }

        public static async Task UpdateCharacterWalletJournal(int characterId)
        {
            var context = new AppDbContext();
            //if (!IsCacheExpired(characterId, RequestHelper.GetEndpointUrl<CharacterWalletJournalModel>())) {
            //    return;
            //}

            var token = Database.GetTokenFor(characterId, EsiCharacterScopes.WalletRead);
            if (token == null) {
                return;
            }

            var access = token.GetAccessToken();
            var result = Esi.WalletApi.GetCharacterWalletJournalCompleteWithInfo(characterId, access);
            foreach (var r in result.Items) {
                if (!context.CharacterWalletJournal.Any(t => (t.Id == r.Id) && (t.CharacterId == characterId))) {
                    context.CharacterWalletJournal.AddOrUpdate(r);
                }
            }

            await context.SaveChangesAsync();
            RegisterCacheTimer(result.Responses.FirstOrDefault());
        }

        public static async Task UpdateCharacterMarketOrdersHistory(int characterId)
        {
            var context = new AppDbContext();
            var token   = Database.GetTokenFor(characterId, EsiCharacterScopes.MarketsOrdersRead);
            if (token == null) {
                return;
            }

            var request = RequestHelper.GetRestRequest<CharacterMarketOrdersHistoryModel>(token.GetAccessToken())
                                       .SetCharacterId(characterId);
            var resource = Esi.Api.BuildUri(request).AbsolutePath;

            if (CacheLogic.IsCacheExpired(resource, "") == false) {
                return;
            }

            var result = Esi.Api.ExecutePaginated<CharacterMarketOrdersHistoryModel>(request);
            foreach (var entry in result.Items) {
                entry.AssignAnnotationFields(request);
                context.CharacterMarketOrdersHistory.AddOrUpdate(entry);
            }

            await context.SaveChangesAsync();
            await RegisterCacheTimer(result.Responses.First());
        }

        public static async Task UpdateCharacterMiningLedger(int characterId)
        {
            var context = new AppDbContext();
            //if (!IsCacheExpired(characterId, RequestHelper.GetEndpointUrl<CharacterMiningLedgerModel>())) {
            //    return;
            //}

            var token = Database.GetTokenFor(characterId, EsiCharacterScopes.IndustryMiningRead);
            if (token == null) {
                return;
            }

            var result = Esi.IndustryApi.GetCharacterMiningLedgerA(characterId, token.GetAccessToken());
            foreach (var r in result.Items) {
                r.AssignAnnotationFields(result.Responses.FirstOrDefault().Request);
                context.CharacterMiningLedger.AddOrUpdate(r);
            }

            await context.SaveChangesAsync();
            await RegisterCacheTimer(result.Responses.FirstOrDefault());
        }

        public static async Task UpdateCharacterWalletTransactions(int characterId)
        {
            var context = new AppDbContext();
            //if (!IsCacheExpired(characterId, RequestHelper.GetEndpointUrl<CharacterWalletTransactionModel>())) {
            //    return;
            //}

            var token = Database.GetTokenFor(characterId, EsiCharacterScopes.MarketsOrdersRead);
            if (token == null) {
                return;
            }

            var result = Esi.WalletApi.GetCharacterWalletTransactionsWithInfo(characterId, Esi.Auth.GetAccessToken(token));
            foreach (var r in result.Data) {
                context.CharacterWalletTransactions.AddOrUpdate(r);
            }

            await context.SaveChangesAsync();
            await RegisterCacheTimer(result);
        }

        public static async Task UpdateCharacterPublicInformation(int characterId)
        {
            var response = RequestHelper.GetRestRequest<CharacterPublicInformation>().SetCharacterId(characterId);
            //if (!IsCacheExpired(characterId, RequestHelper.GetEndpointUrl<CharacterPublicInformation>())) {
            //    return;
            //}

            var context = new AppDbContext();
            var result  = Esi.CharacterApi.GetCharacterPublicInformation(characterId);
            var ci =
                JsonConvert.DeserializeObject<CharacterPublicInformation>(JsonConvert.SerializeObject(result.Data));
            ci.CharacterId = characterId;
            context.CharacterPublicInformation.AddOrUpdate(ci);
            await context.SaveChangesAsync();
        }
    }
}
