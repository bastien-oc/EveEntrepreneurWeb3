using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EntrepreneurCommon.Authentication;
using EntrepreneurCommon.Models;
using EntrepreneurCommon.Models.DatabaseModels;
using EntrepreneurCommon.Models.EsiResponseModels;
using EveEntrepreneurWebPersistency3.Extensions;
using EveEntrepreneurWebPersistency3.Models;

namespace EveEntrepreneurWebPersistency3.Logic
{
    public static class Database
    {
        public static string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder {
                DataSource = "HADES",
                InitialCatalog = "EvePersistency",
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static DbTokenWrapper GetTokenByUuid(string uuid)
        {
            var context = new AppDbContext();
            return context.Tokens.FirstOrDefault(t => t.Uuid == uuid);
        }

        /// <summary>
        ///     Returns the first valid token for a given character with specified scopes.
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public static DbTokenWrapper GetTokenFor(int characterId, params string[] scopes)
        {
            var context = new AppDbContext();
            var result  = context.Tokens.Where(t => t.CharacterId == characterId);
            foreach (var t in result) {
                // Return first token that matches the specified scopes
                if (Esi.Auth.CheckScope(t, scopes)) {
                    return t;
                }
            }

            // If no valid token was found, return null.
            return null;
        }

        public static async Task<string> GetAccesToken(int characterId, params string[] scopes)
        {
            var               context = new AppDbContext();
            var               tokens  = context.Tokens.Where(t => t.CharacterId == characterId);
            DbTokenWrapper token   = null;
            foreach (var t in tokens) {
                if (Esi.Auth.CheckScope(t, scopes)) {
                    token = t;
                }
            }

            var accesssToken = token?.GetAccessToken();
            await context.SaveChangesAsync();
            return accesssToken;
        }

        public static IEnumerable<DbTokenWrapper> GetTokens(int characterId = 0)
        {
            var context = new AppDbContext();
            if (characterId == 0) {
                return context.Tokens;
            }

            var result = context.Tokens.Where(t => t.CharacterId == characterId);
            return result;
        }

        /// <summary>
        ///     Returns the first valid token for a given character with specified scopes.
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public static DbTokenWrapper GetTokenFor(string characterName, params string[] scopes)
        {
            var context = new AppDbContext();
            var result  = context.Tokens.Where(t => t.CharacterName == characterName);
            foreach (var t in result) {
                // Return first token that matches the specified scopes
                if (Esi.Auth.CheckScope(t, scopes)) {
                    return t;
                }
            }

            // If no valid token was found, return null.
            return null;
        }

        /// <summary>
        ///     Refreshes a token and updates it in the database.
        /// </summary>
        /// <param name="token"></param>
        public static void RefreshToken(DbTokenWrapper token)
        {
            var context = new AppDbContext();
            token.RefreshToken();
            context.Tokens.AddOrUpdate((DbTokenWrapper) token);
            context.SaveChanges();
        }

        /// <summary>
        ///     Return a collection of character names the database has tokens for.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetCharacterNames()
        {
            var context    = new AppDbContext();
            var characters = new List<string>();
            foreach (var t in context.Tokens) {
                if (!characters.Contains(t.CharacterName)) {
                    characters.Add(t.CharacterName);
                }
            }

            return characters;
        }

        /// <summary>
        ///     Return a collection of character ids the database has tokens for.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> GetCharacterIds()
        {
            var context    = new AppDbContext();
            var characters = new List<int>();
            foreach (var t in context.Tokens) {
                if (!characters.Contains(t.CharacterId)) {
                    characters.Add(t.CharacterId);
                }
            }

            return characters;
        }

        public static IEnumerable<NameCache> ResolveIdToName(params int[] id)
        {
            var Context = new AppDbContext();
            var lookUp  = new List<int>();

            // For each Id check if it is cached in database and yield return the results.
            // Query all Ids that were not found in the cache.
            foreach (var i in id) {
                if (Context.NameCache.Any(t => t.Id == i)) {
                    yield return Context.NameCache.FirstOrDefault(t => t.Id == i);
                }
                else {
                    lookUp.Add(i);
                }
            }

            // Now look up the missing Ids
            List<UniverseNameResponse> responses;
            //try {
            responses = Esi.UniverseApi.GetUniverseNames(lookUp.ToArray());
            //}
            // There's a chance something will randomly go wrong or the API will not find anything (and will scream).
            //catch (Exception e) {
            //    responses = new List<UniverseNameResponse>();
            //    throw new Exception($"Error while querying API for resolving IDs: {JsonConvert.SerializeObject(id)}",
            //        e);
            //}

            foreach (var r in responses) {
                var entry = new NameCache {
                    Id = r.ID,
                    Name = r.Name,
                    Category = r.Category
                };
                Context.NameCache.AddOrUpdate(entry);
                yield return entry;
            }

            // Context.SaveChanges();

            //// First check the cache.
            //foreach (var i in id) {
            //    if (NameCache.Any(t => t.Id == i))
            //        yield return NameCache.FirstOrDefault(t => t.Id == i);
            //    else {
            //        lookUp.Add(i);
            //    }
            //}

            //// Now look up any and all IDs that we didn't have cached.
            //List<UniverseNameResponse> response;
            //try {
            //    response = Esi.UniverseApi.GetUniverseNames(lookUp.ToArray());
            //}
            //catch (Exception e) {
            //    response = new List<UniverseNameResponse>();
            //}

            //foreach (var r in response) {
            //    var entry = new NameCache() {
            //        Id = r.ID,
            //        Name = r.Name,
            //        Category = r.Category
            //    };
            //    NameCache.Add(entry);
            //    yield return entry;
            //}
        }

        public static NameCache ResolveIdToName(int id)
        {
            var name = ResolveIdToName(new[] {id}).FirstOrDefault();
            return name;
        }
    }
}
