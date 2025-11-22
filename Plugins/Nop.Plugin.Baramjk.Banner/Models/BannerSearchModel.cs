using System;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;

namespace Nop.Plugin.Baramjk.Banner.Models
{
    public partial class BannerSearchModel : ExtendedSearchModel
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string SearchTitle { get; set; }
        public string SearchTag { get; set; }
        public string SearchEntityName { get; set; }
        public int? SearchEntityId { get; set; }
        public DateTime? SearchExpirationDateFrom { get; set; }
        public DateTime? SearchExpirationDateTo { get; set; }
    }
}
