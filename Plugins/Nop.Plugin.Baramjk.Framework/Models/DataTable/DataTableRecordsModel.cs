using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public record DataTableRecordsModel<T>
    {
        public IEnumerable<T> Data { get; set; }

        [JsonProperty(PropertyName = "draw")]
        public int Draw { get; set; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public int RecordsFiltered { get; set; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public int RecordsTotal { get; set; }
    }
}