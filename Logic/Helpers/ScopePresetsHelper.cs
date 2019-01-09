using System;
using System.Collections.Generic;
using System.Linq;
using EntrepreneurCommon.Authentication;
using ESH = EntrepreneurCommon.Authentication.EsiScopesHelper;
using ECS = EntrepreneurCommon.Authentication.EsiCharacterScopes;

namespace EveEntrepreneurWebPersistency3.Logic.Helpers
{
    public static class ScopePresetsHelper
    {
        [EsiScopePreset("Full", "Absolutely most complete API access. Use sparingly and only if you trust the app!")]
        public static string Full = string.Join(" ", ESH.GetScopes(ScopeEntityType.Both));

        [EsiScopePreset("Full (Character)", "Full Character API Access")]
        public static string FullCharacter = string.Join(" ", ESH.GetScopes(ScopeEntityType.Character));

        [EsiScopePreset("Full (Corporation)", "Full Character API Access", EntityScope = ScopeEntityType.Corporation)]
        public static string FullCorporation = string.Join(" ", ESH.GetScopes(ScopeEntityType.Corporation));

        [EsiScopePreset("Full Read-Only", "Grants access to all read-only endpoints for character.")]
        public static string FullReadOnly = string.Join(" ", GetReadOnlyScopes(ScopeEntityType.Character));

        [EsiScopePreset("Industry", "Access to industry jobs and mining ledger")]
        public static string IndustryCharacter = ESH.ParseScopes(ECS.IndustryJobsRead, ECS.IndustryMiningRead);

        [EsiScopePreset("UI Access", "Access to open in-game windows and set waypoints")]
        public static string Ui = ESH.ParseScopes(ECS.UiOpenWindow, ECS.UiWaypointWrite);

        [EsiScopePreset("Wallet", "See character's wallet and journal")]
        public static string WalletCharacter = ESH.ParseScopes(ECS.WalletRead);

        [EsiScopePreset("Assets", "See character's assets")]
        public static string AssetsCharacter = ESH.ParseScopes(ECS.AssetsRead);

        [EsiScopePreset("Character Location",
            "See whether the character is online and where they are in real-time location. See ship type")]
        public static string Location =
            ESH.ParseScopes(ECS.LocationLocationRead, ECS.LocationOnlineRead, ECS.LocationShipTypeRead);

        [EsiScopePreset("Fleet - View", "See the fleet composition and member details")]
        public static string FleetRead = ESH.ParseScopes(ECS.FleetRead);

        [EsiScopePreset("Fleet - Manage", "Allow management of character's fleet")]
        public static string FleetManage = ESH.ParseScopes(ECS.FleetRead, ECS.FleetWrite);

        [EsiScopePreset("Mail - View", "See character's mail")]
        public static string MailRead = ESH.ParseScopes(ECS.MailRead);

        [EsiScopePreset("Mail - Manage", "Organize and send mails")]
        public static string MailManage = ESH.ParseScopes(ECS.MailOrganize, ECS.MailSend);

        [EsiScopePreset("Fittings", "Manage fittings")]
        public static string FittingsManage = ESH.ParseScopes(ECS.FittingsRead, ECS.FittingsWrite);

        [EsiScopePreset("Skills", "See character's skills and current skill queue")]
        public static string Skills = ESH.ParseScopes(ECS.SkillsQueueRead, ECS.SkillsRead);

        [EsiScopePreset("Killmail", "See character's killmail")]
        public static string KillmailCharacter = ESH.ParseScopes(ECS.KillmailsRead);

        [EsiScopePreset("Industry (Corporate)", "See corporation's industry jobs and mining ledger.", EntityScope =
            ScopeEntityType.Corporation)]
        public static string IndustryCorporation =
            ESH.ParseScopes(EsiCorporationScopes.IndustryMiningRead, EsiCorporationScopes.IndustryJobsRead);

        [EsiScopePreset("Assets (Corporate)", "See corporation's assets", EntityScope = ScopeEntityType.Corporation)]
        public static string AssetsCorporation = ESH.ParseScopes(EsiCorporationScopes.AssetsRead);

        [EsiScopePreset("Market", "See character's outstanding orders and recent market transactions history")]
        public static string Market = ESH.ParseScopes(EsiCharacterScopes.MarketsOrdersRead,
            EsiCharacterScopes.MarketsStructureMarkets);

        [EsiScopePreset("Market (Corporate)",
            "See corporation's outstanding orders and recent market transactions history", EntityScope =
                ScopeEntityType.Corporation)]
        public static string MarketCorporation = ESH.ParseScopes(EsiCorporationScopes.MarketsOrdersRead);

        [EsiScopePreset("Persistency",
            "See volatile data for saving into a database (market history, wallet journal and mining ledger")]
        public static string Persistency = ESH.ParseScopes(EsiCharacterScopes.MarketsOrdersRead,
            EsiCharacterScopes.WalletRead, EsiCharacterScopes.IndustryMiningRead);

        [EsiScopePreset("Persistency (Corporate)",
            "See volatile data for saving into a database (market history, wallet journal and mining ledger",
            ScopeEntityType.Corporation)]
        public static string PersistencyCorporation = ESH.ParseScopes(EsiCorporationScopes.MarketsOrdersRead,
            EsiCorporationScopes.WalletRead, EsiCorporationScopes.WalletsRead, EsiCorporationScopes.IndustryMiningRead);


        public static string GetScopesForPreset(string presetName)
        {
            if (presetName.StartsWith("preset")) presetName = presetName.Replace("preset", "");

            var member = typeof(ScopePresetsHelper).GetFields()
                .FirstOrDefault(m => m.Name.ToLower() == presetName.ToLower());

            return member != null ? (string) member.GetValue(null) : "";
        }

        /// <summary>
        /// Parses all known scopes and excludes any scopes that add write access.
        /// </summary>
        /// <param name="entityScope"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetReadOnlyScopes(ScopeEntityType entityScope)
        {
            var scopesRaw = ESH.GetScopes(entityScope);
            foreach (var scope in scopesRaw) {
                if (!scope.ToLower().Contains("write")) yield return scope;
            }
        }
    }

    public class EsiScopePresetAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ScopeEntityType EntityScope { get; set; }

        public EsiScopePresetAttribute(string Name, string Description = "",
            ScopeEntityType Scope = ScopeEntityType.Character)
        {
            this.DisplayName = Name;
            this.Description = Description;
            this.EntityScope = Scope;
        }
    }
}