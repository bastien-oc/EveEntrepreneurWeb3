using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EntrepreneurCommon.Models.EsiResponseModels;
using EveEntrepreneurWebPersistency3.Logic;
using EveEntrepreneurWebPersistency3.Models;
using Newtonsoft.Json;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Controllers
{
    public class InfoController : Controller
    {
        [Route("info/")]
        [Route("info/index/")]
        [Route("info/players/")]
        public ActionResult Index()
        {
            var context = new AppDbContext();
            return View(context.CharacterPublicInformation);
        }

        // GET: Characters
        [Route("info/character/{id}/")]
        public async Task<ActionResult> CharacterPublicInformation(int id)
        {
            var context = new AppDbContext();
            await JobHelper.UpdateCharacterPublicInformation(id);
            var result = context.CharacterPublicInformation.FirstOrDefault(t => t.CharacterId == id);
            if (result == null)
                return HttpNotFound();
            return View(result);
        }

        [Route("info/corporation/{id}/")]
        public ActionResult CorporationInformation(int id)
        {
            //UpdateJobs.UpdateCharacterPublicInformation(characterId);
            //var result = context.CharacterPublicInformation.FirstOrDefault(t => t.CharacterId == characterId);
            //if (result == null)
            //    return HttpNotFound();
            var result = Esi.CorporationApi.GetPublicInformation(id);
            CorporationPublicInformation info =
                JsonConvert.DeserializeObject<CorporationPublicInformation>(JsonConvert.SerializeObject(result.Data));
            info.CorporationId = id;
            return View(info);
        }

        [Route("info/alliance/{id}/")]
        public ActionResult AllianceInformation(int id)
        {
            var info = Esi.CorporationApi.GetAllianceInformation(id).Data;
            return View(info);
        }

        [Route("info/auto/{id}/")]
        public ActionResult DynamicInfo(int id)
        {
            var result = Database.ResolveIdToName(id);
            switch (result.Category) {
                case "character":
                    return RedirectToAction("CharacterPublicInformation", new {id});
                case "corporation":
                    return RedirectToAction("CorporationInformation", new {id});
                case "alliance":
                    return RedirectToAction("AllianceInformation", new {id});
                default:
                    return new EmptyResult();
            }

            //alliance, character, constellation, corporation, inventory_type, region, solar_system, station
        }

        [Route("info/delete/char/{id}/")]
        public ActionResult DeleteInfoC(int id)
        {
            var context = new AppDbContext();
            var data = context.CharacterPublicInformation.FirstOrDefault(t => t.CharacterId == id);
            if (data != null) {
                context.CharacterPublicInformation.Remove(data);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Search(string search = "")
        {
            var request = new RestRequest("/v2/search/", Method.GET);
            request.AddParameter("categories", "character,alliance,corporation", ParameterType.QueryString);
            request.AddParameter("search", search);
            var response = Esi.Api.Execute<SearchResponse>(request);

            ViewBag.Content = response.Content;
            return View(response.Data);
        }
    }
}