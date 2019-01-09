using EntrepreneurCommon.Api;
using EntrepreneurCommon.Client;
using EntrepreneurCommon.Common;
using EveEntrepreneurWebPersistency3.Consts;
using Hangfire.Logging;

namespace EveEntrepreneurWebPersistency3.Models
{
    public static class Esi
    {
        public static EsiAuthClient Auth =
            new EsiAuthClient(ConstEsi.ClientId, ConstEsi.SecretKey, ConstEsi.CallbackUrl) { UserAgent = ConstStrings.DefaultUserAgent };

        public static EsiConfiguration EsiConfiguration = new EsiConfiguration()
        {
            CallbackUrl = ConstEsi.CallbackUrl,
            ClientId = ConstEsi.ClientId,
            SecretKey = ConstEsi.SecretKey,
            DataSource = ConstStrings.DefaultDatasource,
            UserAgent = ConstStrings.DefaultUserAgent
        };


        public static IEsiRestClient Api = new EsiRestClient(configuration: EsiConfiguration);

        public static CommonApi CommonApi = new CommonApi(Api);
        public static CharacterApi CharacterApi = new CharacterApi(Api);
        public static IndustryApi IndustryApi = new IndustryApi(Api);
        public static WalletApi WalletApi = new WalletApi(Api);
        public static MarketApi MarketApi = new MarketApi(Api);
        public static UniverseApi UniverseApi = new UniverseApi(Api);
        public static CorporationApi CorporationApi = new CorporationApi(Api);

        public static string GetRedirectUrl(string scopes = "", string state = null)
        {
            return Auth.GetRedirectUrl(scopes, state);
        }
    }
}