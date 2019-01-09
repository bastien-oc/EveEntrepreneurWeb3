using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Common.Attributes;
using EntrepreneurCommon.ExtensionMethods;
using EntrepreneurCommon.Helpers;
using EntrepreneurCommon.Models.EsiResponseModels;
using EveEntrepreneurWebPersistency3.Extensions;
using EveEntrepreneurWebPersistency3.Logic;
using EveEntrepreneurWebPersistency3.Models;
using Newtonsoft.Json;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Controllers
{
    public class HomeController : Controller
    {
        private string _content;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> DevTest()
        {
            //return Content("Hello World!");
            //return HttpNotFound();
            //return new EmptyResult();
            //return RedirectToAction("Redirect", "Sso", new {act = "GetToken"});
            //return Redirect()
            //return RedirectToAction("ScopeSelector", "Tokens");

            //string jsonstring =
            //    "{\r\n    \"amount\": -35000000,\r\n    \"balance\": 144281302.13,\r\n    \"context_id\": 4983979866,\r\n    \"context_id_type\": \"market_transaction_id\",\r\n    \"date\": \"2018-10-27T15:16:54Z\",\r\n    \"description\": \"Market escrow release\",\r\n    \"first_party_id\": 2112291012,\r\n    \"id\": 16085615855,\r\n    \"ref_type\": \"market_escrow\",\r\n    \"second_party_id\": 2112291012\r\n}";
            //List<Parameter> parameters = new List<Parameter>() {
            //    new Parameter(){ Name = "character_id", Type = ParameterType.UrlSegment, Value = (int)2112291012}
            //};

            //var responseData = JsonConvert.DeserializeObject<WalletJournalModelCharV4>(jsonstring);
            //responseData.AssignAnnotationFields<WalletJournalModelCharV4>(parameters.ToArray());

            //string stuff = responseData.GetEndpointUrl<WalletJournalModelCharV4>();

            //return Content($"<h1><small>{stuff}</small></h1><pre>{JsonConvert.SerializeObject(responseData,Formatting.Indented)}</pre>");
            //var url = (IEsiEndpoint)CharacterPublicInformation.
            var ResourceUri = RequestHelper.GetEndpointUrl<CorporationMarketOrdersHistory>();
            var corpId = 1720036125;
            var characterId = 94271081;


            var request = new RestRequest(ResourceUri);
            request.AddParameter("corporation_id", corpId, ParameterType.UrlSegment);
            var token = Database.GetTokenFor(94271081, EsiCorporationScopes.MarketsOrdersRead).GetAccessToken();
            //request.AddParameter("token", token);
            //request.AddParameter("Authorization", $"Bearer {token}", ParameterType.HttpHeader);
            request.SetToken(token);

            var cacheKey = Esi.Api.BuildUri(request).AbsolutePath;
            var thing = Esi.Api.ExecutePaginated<CorporationMarketOrdersHistory>(request);

            var context = new AppDbContext();
            foreach (var i in thing.Items)
            {
                i.AssignAnnotationFields(thing.Responses.First().Request.Parameters.ToArray());
                context.CorporationMarketOrdersHistory.AddOrUpdate(i);
            }

            await context.SaveChangesAsync();

            _content =
                $"<pre>Calling endpoint: {ResourceUri}</br>Caching key: {cacheKey}</br>Cache Expires: {thing.Responses.FirstOrDefault().GetCacheExpiry()} </br>{JsonConvert.SerializeObject(thing.Items, Formatting.Indented)}</br></pre>";
            return Content(
                _content);
            //var request = new RestRequest((IEsiEndpoint)CharacterPublicInformation.)
        }
    }
}