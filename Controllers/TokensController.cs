using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Models.DatabaseModels;
using EntrepreneurCommon.Models.EsiResponseModels;
using EveEntrepreneurWebPersistency3.Logic;
using EveEntrepreneurWebPersistency3.Models;
using Newtonsoft.Json;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Controllers
{
    public class TokensController : Controller
    {
        // GET: Tokens
        [Route("tokens/")]
        public ActionResult Index()
        {
            var context = new AppDbContext();
            var tokens = context.Set<DbTokenWrapper>();
            return View(tokens);
        }

        [Route("tokens/characters")]
        public ActionResult IndexByCharacters()
        {
            var tokens = new AppDbContext();
            var ids = Database.GetCharacterIds();
            List<CharacterPublicInformation> cpi = new List<CharacterPublicInformation>();
            foreach (var id in Database.GetCharacterIds()) {
                JobHelper.UpdateCharacterPublicInformation(id);
                cpi.Add(tokens.CharacterPublicInformation.FirstOrDefault(info => info.CharacterId == id));
            }

            return View(cpi);
        }

        [Route("tokens/details/{uuid}/")]
        public ActionResult Details(string uuid)
        {
            var context = new AppDbContext();
            var token = context.Tokens.First(t => t.Uuid == uuid);
            if (token == null) {
                return RedirectToAction("Index");
            }
            return View(token);
        }

        [Route("tokens/json/{uuid}/")]
        public ActionResult AsJson(string uuid)
        {
            var Token = Database.GetTokenByUuid(uuid);
            Response.AppendHeader("Content-Disposition",$"{Token.CharacterName}");
            var bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(Token));
            var cd = new System.Net.Mime.ContentDisposition() {
                FileName = $"{Token.CharacterName}-{Token.Uuid}.json",
                Inline = false,
            };
            return File(bytes, cd.ToString());
            return Json(Token, JsonRequestBehavior.AllowGet);
        }

        [Route("tokens/select_scopes")]
        public ActionResult ScopeSelector()
        {
            return View();
        }

        [Route("tokens/delete/{uuid}/")]
        public async Task<ActionResult> DeleteToken(string uuid)
        {
            var context = new AppDbContext();
            var token = context.Tokens.FirstOrDefault(t => t.Uuid == uuid);
            if (token == null) {
                return HttpNotFound($"Esi token with UUID {uuid} was not found.");
            }

            var result = Esi.Auth.RevokeToken(TokenAuthenticationType.RefreshToken, token.RefreshToken);
            if (result) {
                context.Tokens.Remove(token);
                context.SaveChanges();
            }
            else return Content("Something went wrong.");

            return RedirectToAction("Index");
        }

        [Route("tokens/test/{uuid}/")]
        public ActionResult TestToken(string uuid)
        {
            return new EmptyResult();
        }

        [Route("tokens/refresh/{uuid}/")]
        public ActionResult Refresh(string uuid)
        {
            var token = Database.GetTokenByUuid(uuid);
            Database.RefreshToken(token);
            return RedirectToAction("Index");
        }
    }
}