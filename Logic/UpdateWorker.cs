using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Helpers;
using EntrepreneurCommon.Models.EsiResponseModels;
using EveEntrepreneurWebPersistency3.Models;
using Hangfire;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Logic
{
    public class UpdateWorker
    {
        public string           ResourceUri      { get; set; }
        public string           ResourceResolved { get; }
        public List<Parameter>  Parameters       { get; set; }
        public DateTime   NextUpdate       { get; set; }
        public UpdateWorkerType WorkType         { get; }
        public DateTime   LastUpdate       { get; set; }
        public bool             Cancel           { get; set; } = false;

        public UpdateWorker(UpdateWorkerType workType, params Parameter[] parameters)
        {
            WorkType = workType;
            Parameters = parameters.ToList();

            switch (workType) {
                case UpdateWorkerType.CharacterWalletJournal:
                    ResourceUri = RequestHelper.GetEndpointUrl<CharacterWalletJournalModel>();
                    //UpdateMethod = JobHelper.UpdateCharacterWalletJournal;
                    break;
                case UpdateWorkerType.CharacterWalletTransactions:
                    ResourceUri = RequestHelper.GetEndpointUrl<CharacterWalletTransactionModel>();
                    //UpdateMethod = JobHelper.UpdateCharacterWalletTransactions;
                    break;
                case UpdateWorkerType.CharacterMarketOrdersHistory:
                    ResourceUri = RequestHelper.GetEndpointUrl<CharacterMarketOrdersHistoryModel>();
                    //UpdateMethod = JobHelper.UpdateCharacterMarketOrdersHistory;
                    break;
                case UpdateWorkerType.CharacterMiningJournal:
                    ResourceUri = RequestHelper.GetEndpointUrl<CharacterMiningLedgerModel>();
                    //UpdateMethod = JobHelper.UpdateCharacterMiningLedger;
                    break;
                case UpdateWorkerType.CorporationWalletJournal:
                    ResourceUri = RequestHelper.GetEndpointUrl<CorporationWalletJournalModel>();
                    throw new NotImplementedException();
                case UpdateWorkerType.CorporationWalletTransactions:
                    ResourceUri = RequestHelper.GetEndpointUrl<CorporationWalletTransactionModel>();
                    throw new NotImplementedException();
                case UpdateWorkerType.CorporationMarketOrdersHistory:
                    ResourceUri = RequestHelper.GetEndpointUrl<CorporationMarketOrdersHistory>();
                    throw new NotImplementedException();
                case UpdateWorkerType.CorporationMiningDone:
                    ResourceUri = RequestHelper.GetEndpointUrl<CorporationMiningObserverLedgerModel>();
                    throw new NotImplementedException();
            }

            var request = new RestRequest(ResourceUri);
            foreach (var p in Parameters) {
                request.AddParameter(p);
            }

            ResourceResolved = Esi.Api.BuildUri(request).AbsolutePath;
        }

        private int GetCharacterId()
        {
            return (int) Parameters.First(p => p.Name == "character_id").Value;
        }

        private int GetCorporationId()
        {
            return (int) Parameters.First(p => p.Name == "corporation_id").Value;
        }

        private int GetWalletDivision()
        {
            return (int) Parameters.First(p => p.Name == "wallet_division").Value;
        }

        public async void ThreadPoolCallback(Object threadContext)
        {
            var context = new AppDbContext();
            int i       = 1;
            while (Cancel == false) {
                var timer = context.CacheTimers.FirstOrDefault(t =>
                                                                   (t.Resource   == ResourceResolved)
                                                                && (t.DataSource == Esi.EsiConfiguration.DataSource)
                                                                && (t.Key        == ""));

                // Check timer.
                if (CacheLogic.IsCacheExpired(ResourceResolved, "")) {
                    //Task.Run(() => Work()).Wait();
                    await DelegateWork();
                    Thread.Sleep(5000);
                    if (timer == null) {
                        // If the timer is still not present after fifteen attempts to wait, throw exception.
                        if (i >= 15) {
                            throw new Exception("Cannot get the timer for the job.");
                        }

                        // If the timer isn't there, give it increasingly more time to spawn, then try anew.
                        Thread.Sleep(i * 1000);
                        i++;
                        continue;

                        //Cancel = true;
                        //throw new Exception($"Something went wrong, no cache timer was found for the job. {this}");
                    }

                    LastUpdate = timer.LastUpdated;
                    NextUpdate = timer.Expires + TimeSpan.FromMinutes(1);
                    var delay = (timer.Expires - DateTime.UtcNow) + TimeSpan.FromMinutes(1);
                    if (delay > TimeSpan.Zero) {
                        Thread.Sleep(delay);
                    }
                }
                else {
                    NextUpdate = timer.Expires + TimeSpan.FromMinutes(1);
                }
            }
        }

        public async Task DelegateWork()
        {
            switch (WorkType) {
                case UpdateWorkerType.CharacterWalletJournal:
                    await JobHelper.UpdateCharacterWalletJournal(GetCharacterId());
                    break;
                case UpdateWorkerType.CharacterWalletTransactions:
                    await JobHelper.UpdateCharacterWalletTransactions(GetCharacterId());
                    break;
                case UpdateWorkerType.CharacterMarketOrdersHistory:
                    await JobHelper.UpdateCharacterMarketOrdersHistory(GetCharacterId());
                    break;
                case UpdateWorkerType.CharacterMiningJournal:
                    await JobHelper.UpdateCharacterMiningLedger(GetCharacterId());
                    break;
                case UpdateWorkerType.CorporationWalletJournal:
                    throw new NotImplementedException();
                case UpdateWorkerType.CorporationWalletTransactions:
                    throw new NotImplementedException();
                case UpdateWorkerType.CorporationMarketOrdersHistory:
                    throw new NotImplementedException();
                case UpdateWorkerType.CorporationMiningDone:
                    throw new NotImplementedException();
            }
        }
    }
}
