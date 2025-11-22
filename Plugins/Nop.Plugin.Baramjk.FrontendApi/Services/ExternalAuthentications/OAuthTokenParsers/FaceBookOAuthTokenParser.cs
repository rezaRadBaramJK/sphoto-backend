using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.Validators;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications.OAuthTokenParsers
{
    public class FaceBookOAuthTokenParser : BaseOAuthTokenParser
    {
        private const string url =
            "https://graph.facebook.com/me?fields=gender,birthday,picture.type(large),name,email,first_name,last_name&access_token=";

        public FaceBookOAuthTokenParser(IdTokenValidatorService idTokenValidatorService) : base(idTokenValidatorService)
        {
        }

        protected override OAuthProvider AuthProvider => OAuthProvider.Facebook;

        public override async Task<OAuthTokenDataResult> GetOAuthTokenDataAsync(SignInModel model)
        {
            var userInfo = await FetchData(model.AccessToken);
            if (userInfo == null)
                return new OAuthTokenDataResult();

            var birthdayValid = DateTime.TryParse(userInfo.birthday, out var birthday);

            var avatar = userInfo?.picture?.data?.url;
            var oAuthTokenData = new OAuthTokenData
            {
                FirstName = userInfo.first_name,
                LastName = userInfo.last_name,
                ExternalDisplayIdentifier = userInfo.name,
                ExternalIdentifier = userInfo.id,
                Birthdate = birthdayValid ? birthday : null,
                Email = userInfo.email,
                AccessToken = model.AccessToken,
                Avatar = avatar
            };

            FillByUserInfo(model?.UserInfo, oAuthTokenData);

            var authTokenDataResult = new OAuthTokenDataResult
            {
                IsSuccess = true,
                ValidateResult = new ValidateResult(),
                OAuthTokenData = oAuthTokenData
            };

            return authTokenDataResult;
        }

        private static async Task<UserInfo> FetchData(string accessToken)
        {
            try
            {
                using var client = new HttpClient();
                var userInfoUrl = $"{url}{accessToken}";
                var userInfoJson = await client.GetStringAsync(userInfoUrl);
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoJson);
                return userInfo;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public class UserInfo
        {
            public string name { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string id { get; set; }
            public string birthday { get; set; }
            public Picture picture { get; set; }
        }

        public class Data
        {
            public int height { get; set; }
            public bool is_silhouette { get; set; }
            public string url { get; set; }
            public int width { get; set; }
        }

        public class Picture
        {
            public Data data { get; set; }
        }
    }
}