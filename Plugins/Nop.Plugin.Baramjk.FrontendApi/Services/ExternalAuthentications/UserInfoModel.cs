using System;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.ExternalAuthentications
{
    public class UserInfoModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Gender { get; set; }
        public string MobileNumber { get; set; }
        public string Avatar { get; set; }
    }
}