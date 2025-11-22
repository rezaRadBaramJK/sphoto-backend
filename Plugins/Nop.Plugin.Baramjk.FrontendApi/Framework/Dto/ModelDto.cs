using System.Collections.Generic;
using Nop.Plugin.Baramjk.Framework.Models;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Dto
{
    public abstract class ModelDto : BaseDto, ICustomProperties
    {
        /// <summary>
        ///     Gets or sets property to store any custom values for models
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; }
    }
}