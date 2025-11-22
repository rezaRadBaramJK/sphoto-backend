using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Customer
{
    public class InfoResponse : BaseDto
    {
        public CustomerInfoModelDto Model { get; set; }

        public IList<string> Errors { get; set; }
    }
}