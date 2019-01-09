using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Models.DatabaseModels;
using EveEntrepreneurWebPersistency3.Models;

namespace EveEntrepreneurWebPersistency3.Extensions
{
    public static class EsiTokenContainerExtensions
    {
        public static string GetAccessToken(this DbTokenWrapper token)
        {
            var context = new AppDbContext();
            var result = Esi.Auth.GetAccessToken(token);
            context.Tokens.AddOrUpdate(token);
            context.SaveChanges();
            return result;
        }

        public static IEsiTokenContainer RefreshToken(this DbTokenWrapper token)
        {
            var context = new AppDbContext();
            var result = Esi.Auth.RefreshToken(token);
            context.Tokens.AddOrUpdate(token);
            context.SaveChanges();
            return result;
        }
    }
}