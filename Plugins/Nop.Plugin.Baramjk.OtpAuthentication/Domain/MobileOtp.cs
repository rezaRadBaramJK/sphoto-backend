using System;
using Nop.Core;
using Nop.Plugin.Baramjk.OtpAuthentication.Consts;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Domain
{
    public class MobileOtp : BaseEntity
    {
        public string PhoneNumber { get; set; }
        public string OldPhoneNumber { get; set; }
        public string Otp { get; set; }
        public OtpType OtpType { get; set; }
        public DateTime CreateDateTime { get; set; }
        public int AttemptNumber { get; set; }
    }
}