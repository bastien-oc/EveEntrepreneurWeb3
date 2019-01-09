using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Common;
using EntrepreneurCommon.Models;
using EveEntrepreneurWebPersistency3.Consts;
using EveEntrepreneurWebPersistency3.Logic;
using EveEntrepreneurWebPersistency3.Models;
using CacheTimer = EveEntrepreneurWebPersistency3.Models.CacheTimer;

namespace EveEntrepreneurWebPersistency3.Controllers.Api
{
    public class SystemApiController : ApiController
    {
        [Route("api/tokens/{characterId}")]
        public IEnumerable<EsiTokenContainer> GetTokens(int characterId)
        {
            var tokens = Database.GetTokens();
            return tokens;
        }

        [Route("api/token/{characterId}")]
        public EsiTokenContainer GetToken(int characterId, string scopes)
        {
            return Database.GetTokenFor(characterId, scopes);
        }

        [Route("api/token/{characterId}/access_token")]
        public async Task<string> GetAccessToken(int characterId, string scopes)
        {
            return await Database.GetAccesToken(characterId, scopes);


        }

        // GET: api/SystemApi
        [Route("api/SystemApi")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SystemApi/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/SystemApi
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/SystemApi/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SystemApi/5
        public void Delete(int id)
        {
        }

        [Route("api/timers"), HttpGet]
        public IEnumerable<CacheTimer> GetTimers()
        {
            var context = new AppDbContext();
            foreach (var c in context.CacheTimers) {
                yield return c;
            }
        }

        

        [Route("api/timers/{id}"), HttpGet]
        public IEnumerable<CacheTimer> GetTimer(int id, string resource, string datasource = ConstStrings.DefaultDatasource, string key = "")
        {
            return new AppDbContext().CacheTimers.Where(e => e.Resource == resource);
        }

        [Route("api/name/{id}"), HttpGet]
        public NameCache ResolveName(int id)
        {
            return Database.ResolveIdToName(id);
        }
    }
}
