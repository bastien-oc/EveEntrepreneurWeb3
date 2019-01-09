using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.DbLayer;
using EveEntrepreneurWebPersistency3.Logic;
using EveEntrepreneurWebPersistency3.Models;
using Hangfire;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Controllers
{
    public class PersistencyController : Controller
    {
        public ActionResult Index()
        {
            var context = new AppDbContext();
            if (!Request.IsAuthenticated) return RedirectToAction("LogIn", "Account");
            return View(context.Tokens);
        }

        // GET: Industry
        [Route("persistency/ledger/{characterId}")]
        public async Task<ActionResult> MiningLedger(int characterId)
        {
            var context = new AppDbContext();
            if (!Request.IsAuthenticated) return RedirectToAction("LogIn", "Account");
            await JobHelper.UpdateCharacterMiningLedger(characterId);
            return View(context.CharacterMiningLedger
                               .Where(t => t.CharacterId == characterId)
                               .OrderByDescending(t => t.Date));
        }

        [Route("persistency/wallet/{characterId}")]
        public ActionResult WalletJournal(int characterId)
        {
            ViewBag.Id = characterId;

            var context = new AppDbContext();
            if (!Request.IsAuthenticated) return RedirectToAction("LogIn", "Account");
            JobHelper.UpdateCharacterWalletJournal(characterId);
            var result = context.CharacterWalletJournal
                                .Where(t => t.CharacterId == characterId)
                                .OrderByDescending(t => t.Date);
            return View(result);
        }

        [Route("persistency/wallet_transactions/{characterId}")]
        public ActionResult WalletTransactions(int characterId)
        {
            var context = new AppDbContext();
            if (!Request.IsAuthenticated) return RedirectToAction("LogIn", "Account");
            JobHelper.UpdateCharacterWalletTransactions(characterId);
            var result = context.CharacterWalletTransactions
                                .Where(t => t.CharacterId == characterId)
                                .OrderByDescending(t => t.Date);
            return View(result);
        }

        [Route("persistency/order_history/{characterId}")]
        public ActionResult MarketOrdersHistory(int characterId)
        {
            var context = new AppDbContext();
            if (!Request.IsAuthenticated) {
                return RedirectToAction("LogIn", "Account");
            }

            JobHelper.UpdateCharacterMarketOrdersHistory(characterId);
            var result = context.CharacterMarketOrdersHistory
                                .Where(t => t.CharacterId == characterId)
                                .OrderByDescending(t => t.Issued);
            return View(result);
        }

        [Route("persistency/cache_timers")]
        public ActionResult CacheTimers()
        {
            var context = new AppDbContext();
            if (!Request.IsAuthenticated) return RedirectToAction("LogIn", "Account");
            return View(context.CacheTimers);
        }

        public ActionResult Spawn()
        {
            var characters = Database.GetCharacterIds();
            foreach (var cid in characters) {
                if (Database.GetTokenFor(cid, new[] {EsiCharacterScopes.WalletRead}) != null) {
                    var parameters = new Parameter[] {new Parameter("character_id", cid, ParameterType.UrlSegment)};
                    var jobJournal = new UpdateWorker(UpdateWorkerType.CharacterWalletJournal, parameters);
                    JobHelper.UpdateWorkers.Add(jobJournal);
                    ThreadPool.QueueUserWorkItem(jobJournal.ThreadPoolCallback);
                    var jobTransactions = new UpdateWorker(UpdateWorkerType.CharacterWalletTransactions, parameters);
                    JobHelper.UpdateWorkers.Add(jobTransactions);
                    ThreadPool.QueueUserWorkItem(jobTransactions.ThreadPoolCallback);
                }

                if (Database.GetTokenFor(cid, new[] {EsiCharacterScopes.IndustryMiningRead}) != null) {
                    var parameters = new Parameter[] {new Parameter("character_id", cid, ParameterType.UrlSegment)};
                    var jobLedger  = new UpdateWorker(UpdateWorkerType.CharacterMiningJournal, parameters);
                    JobHelper.UpdateWorkers.Add(jobLedger);
                    ThreadPool.QueueUserWorkItem(jobLedger.ThreadPoolCallback);
                }

                if (Database.GetTokenFor(cid, new[] {EsiCharacterScopes.MarketsOrdersRead}) != null) {
                    var parameters = new Parameter[]
                        {new Parameter("character_id", cid, ParameterType.UrlSegment)};
                    var jobMarketHistory = new UpdateWorker(UpdateWorkerType.CharacterMarketOrdersHistory, parameters);
                    JobHelper.UpdateWorkers.Add(jobMarketHistory);
                    ThreadPool.QueueUserWorkItem(jobMarketHistory.ThreadPoolCallback);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
