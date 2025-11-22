using System.Text.Json.Serialization;

namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public class DeleteRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}