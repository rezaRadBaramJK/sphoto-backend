using System;

namespace Nop.Plugin.Baramjk.Framework.Providers.ProtectedEntities.Models
{
    public class ProtectedEntityItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
    }
}