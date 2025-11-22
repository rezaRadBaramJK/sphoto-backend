using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;

namespace Nop.Plugin.Baramjk.Banner.Data.Mapping
{
    public class NameCompatability : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            {
                typeof(Slide),
                "SS_AS_Slide"
            },
            {
                typeof(Slider),
                "SS_AS_AnywhereSlider"
            },
            {
                typeof(EntityWidgetMapping),
                "SS_MAP_EntityWidgetMapping"
            }
        };

        public Dictionary<(Type, string), string> ColumnName => new();
    }
}