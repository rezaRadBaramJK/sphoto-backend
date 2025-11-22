using System.Collections.Generic;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto
{
    public class BaseModelDtoRequest<ModelType> : BaseDto
        where ModelType : BaseDto
    {
        public ModelType Model { get; set; }

        public IDictionary<string, string> Form { get; set; }
    }
}