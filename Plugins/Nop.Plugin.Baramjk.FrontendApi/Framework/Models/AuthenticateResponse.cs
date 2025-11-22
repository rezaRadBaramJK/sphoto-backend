using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Models
{
    public class AuthenticateResponse : BaseDto
    {
        public string Username { get; set; }

        public int CustomerId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Token { get; set; }

        public List<RoleItem> Roles { get; set; }
        public DateTimeOffset ExpireDate { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RoleItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}