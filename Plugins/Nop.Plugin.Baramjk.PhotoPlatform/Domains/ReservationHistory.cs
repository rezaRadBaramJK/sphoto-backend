using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Domains
{
    public class ReservationHistory : BaseEntity
    {
        public int ReservationId { get; set; }
        public int LastChangedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string Changes { get; set; }
    }

    public class PropertyChange
    {
        public int ChangedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
        public object Old { get; set; }
        public object New { get; set; }
    }
}