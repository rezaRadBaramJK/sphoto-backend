using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Wishlist
{
    public class WishlistEmailAFriendModelDto : ModelDto
    {
        public string FriendEmail { get; set; }

        public string YourEmailAddress { get; set; }

        public string PersonalMessage { get; set; }

        public bool SuccessfullySent { get; set; }

        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}