using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Models.Central;
using EntrepreneurCommon.Models.DatabaseModels;
using EveEntrepreneurWebPersistency3.Logic.Helpers;
using EveEntrepreneurWebPersistency3.Models;

namespace EveEntrepreneurWebPersistency3.Controllers
{
    public class SsoController : Controller
    {
        private static readonly List<DbTokenWrapper> TokenCache = new List<DbTokenWrapper>();

        // GET: Sso
        public ActionResult Callback()
        {
            // Create a callback model with all the relevant information
            var callback = new SsoCallbackModel {
                SsoCode = Request.Params["code"],
                State = Request.Params["state"] ?? "add"
            };

            var token        = Esi.Auth.GetTokenComposite(callback.SsoCode, TokenAuthenticationType.VerifyAuthCode);
            var tokenWrapper = new DbTokenWrapper(token);
            callback.TokenUuid = tokenWrapper.Uuid;
            TokenCache.Add(tokenWrapper);

            var status = callback.State;

            switch (status) {
                case "add":
                    return RedirectToAction("AddTokenToDb", callback);
                case "signup":
                    return RedirectToAction("SignUpStepComplete", callback);
                case "signin":
                    return RedirectToAction("SignIn", callback);
                default:
                    return RedirectToAction("AddTokenToDb", callback);
            }
        }

        public async Task<ActionResult> AddTokenToDb(SsoCallbackModel callback)
        {
            var context = new AppDbContext();
            var token   = TokenCache.FirstOrDefault(t => t.Uuid == callback.TokenUuid);
            context.Tokens.Add(token);
            await context.SaveChangesAsync();
            TokenCache.Remove(token);
            return View(callback);
        }

        [HttpPost]
        public ActionResult RedirectToSso( /*List<string> scopes*/ FormCollection obj)
        {
            // var debug = obj.AllKeys.Aggregate("", (current, k) => current + $"{k}: {obj[k]}<br/>");

            var scopes = new List<string>();

            // Register individually selected scopes
            if (obj.AllKeys.Contains("scopes-character")) {
                scopes.AddRange(obj["scopes-character"].Split(','));
            }

            if (obj.AllKeys.Contains("scopes-corporation")) {
                scopes.AddRange(obj["scopes-corporation"].Split(','));
            }

            // Register presets
            var presets = obj.AllKeys.Where(k => k.StartsWith("preset"));

            // Process presets
            foreach (var p in presets) {
                var presetResponse = ScopePresetsHelper.GetScopesForPreset(p);
                if (presetResponse == "") {
                    continue; // Preset doesn't exist or didn't register any scopes
                }

                // Get individual scopes from preset and individually add them to list of scopes if they aren't there already.
                var presetResponseScopes = presetResponse.Split(' ');
                foreach (var presetScope in presetResponseScopes) {
                    if (!scopes.Contains(presetScope)) {
                        scopes.Add(presetScope);
                    }
                }
            }

            var redirectUrl = Esi.GetRedirectUrl(string.Join(" ", scopes), "add");
            return Redirect(redirectUrl);
            // DEBUG
            //dynamic data = new {
            //    _scopes = scopes,
            //    _presets = presets,
            //    _indy = ScopePresetsHelper.GetScopesForPreset("presetCharacterIndustry"),
            //    _debug = _debug,
            //    redirect_string = EsiApiModule.GetRedirectUrl(String.Join(" ",scopes))
            //};
            //return Content($"<pre>{JsonConvert.SerializeObject(data, Formatting.Indented)}</pre>");
        }

        [Route("sso/signup/")]
        public ActionResult SignUpInit()
        {
            if (Request.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(Esi.GetRedirectUrl(state: "signup"));
        }

        [Route("sso/signup/continue")]
        public ActionResult SignUpStepComplete(SsoCallbackModel callback)
        {
            ViewBag.CallBack = callback;
            var token = TokenCache.FirstOrDefault(t => t.Uuid == callback.TokenUuid);

            var entity = new UserEntity {
                MainCharacter = token?.CharacterId ?? 0
            };
            return View(entity);
        }

        [Route("sso/signup/continue")]
        [HttpPost]
        public ActionResult SignUpStepComplete(UserEntity model)
        {
            if (ModelState.IsValid) {
                var context = new AppDbContext();
                context.Users.AddOrUpdate(model);
                context.SaveChanges();
            }


            return RedirectToAction("Index", "Home");
        }
    }
}
