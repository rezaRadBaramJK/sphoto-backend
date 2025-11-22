using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework
{
    public static class FrameworkDefaultValues
    {
        public static List<string> EmptyStringList = new();

        public const string ConfigureLayoutViewPath = "~/Plugins/Baramjk.Core/Views/Shared/_ConfigureLayout.cshtml";

        public const string AdminLayoutViewPath =  "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";
        public const string FormLayoutViewPath =  "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";

        
        public const string ListLayoutViewPath = "~/Plugins/Baramjk.Core/Views/Shared/_ListLayout.cshtml";
        public const string DefaultListViewPath = "~/Plugins/Baramjk.Core/Views/Shared/DefaultList.cshtml";

        public const string ProductItemsEventTopic = "productItems";
        public const string ProductDetailsModelsEventTopic = "ProductDetailsModel";
        public const string ProductDetailsEventTopic = "productDetails";
        
        public const string JCarouselCategoryEventTopic = "JCarouselCategory";
    }
}