using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EveEntrepreneurWebPersistency3.Models;
using RestSharp;

namespace EveEntrepreneurWebPersistency3.Logic
{
    public static class CacheLogic
    {
        public static CacheTimer CreateCacheTimer(IRestResponse response)
        {
            return new CacheTimer() {
                Resource = Esi.Api.BuildUri(response.Request).AbsolutePath,
                DataSource = Esi.EsiConfiguration.DataSource,
                Key = "",
                Expires =
                    DateTime
                       .Parse((string) response.Headers.FirstOrDefault(t => t.Name == "Expires")?.Value),
                LastUpdated = DateTime.UtcNow
            };
        }

        public static bool IsCacheExpired(string resource, string key)
        {
            var context = new AppDbContext();
            var timer   = context.CacheTimers.FirstOrDefault(t => t.Resource == resource && t.Key == key);
            if (timer == null)
            {
                return true;
            }

            var now = DateTime.UtcNow;
            var exp = timer.Expires;
            return now > exp ? true : false;
        }
    }
}
