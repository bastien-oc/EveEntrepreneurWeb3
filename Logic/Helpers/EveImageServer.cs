using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EveEntrepreneurWebPersistency3.Logic.Helpers
{
    public static class EveImageServer
    {
        public static string BaseUrl => "https://imageserver.eveonline.com";
        public static string CharacterUrlTemplate => BaseUrl + "/Character/{characterID}_{width}.jpg";
        public static string CorporationUrlTemplate => BaseUrl + "/Corporation/{corpID}_{width}.png";
        public static string AllianceUrlTemplate => BaseUrl + "/Alliance/{allianceID}_{width}.png";

        public static string FactionUrlTemplate => BaseUrl + "/Alliance/{factionID}_{width}.png";

        public static string InventoryUrlTemplate => BaseUrl + "/Type/{typeID}_{width}.png";
        public static string ShipRenderUrlTemplate => BaseUrl + "/Render/{typeID}_{width}.png";

        public static string GetCharacterImage(int characterId, CharacterImageSize size) => CharacterUrlTemplate
            .Replace("{characterID}", characterId.ToString())
            .Replace("{width}", ((int) size).ToString());

        public static string GetCorporationImage(int corporationId, CorporationImageSize size) => CorporationUrlTemplate
            .Replace("{corpID}", corporationId.ToString())
            .Replace("{width}", ((int) size).ToString());

        public static string GetAllianceImage(int allianceId, AllianceImageSize size) =>
            AllianceUrlTemplate
                .Replace("{allianceID}", allianceId.ToString())
                .Replace("{width}", ((int) size).ToString());

        public static string GetInventoryImage(int typeId, InventoryImageSize size) =>
            InventoryUrlTemplate
                .Replace("{typeID}", typeId.ToString())
                .Replace("{width}", ((int) size).ToString());
    }

    public enum CharacterImageSize
    {
        Res32 = 32,
        Res64 = 64,
        Res128 = 128,
        Res256 = 256,
        Res512 = 512,
        Res1024 = 1024
    }

    public enum AllianceImageSize
    {
        Res32 = 32,
        Res64 = 64,
        Res128 = 128
    }

    public enum CorporationImageSize
    {
        Res32 = 32,
        Res64 = 64,
        Res128 = 128,
        Res256 = 256
    }

    public enum InventoryImageSize
    {
        Res32 = 32,
        Res64 = 64
    }

    public enum ShipAndDroneImageSize
    {
        Res32 = 32,
        Res64 = 64,
        Res128 = 128,
        Res256 = 256,
        Res512 = 512
    }
}