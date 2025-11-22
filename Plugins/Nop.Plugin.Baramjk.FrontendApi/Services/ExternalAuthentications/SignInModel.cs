namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications
{
    public class SignInModel
    {
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string ClientId { get; set; }
        public OAuthProvider Provider { get; set; }
        public UserInfoModel UserInfo { get; set; }
    }
}