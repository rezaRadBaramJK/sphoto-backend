using System;
using Nop.Core;

namespace Nop.Plugin.Baramjk.Framework.Domain
{
    public class ActionLog : BaseEntity
    {
        public ActionLog()
        {
        }

        public ActionLog(string group = "", string action = "", int customerId = default, int entityId = default,
            string value = "", string meta = "")
        {
            Group = group;
            Action = action;
            CustomerId = customerId;
            EntityId = entityId;
            Value = value;
            Meta = meta;
            CreatedOnUtc = DateTime.UtcNow;
        }

        public string Group { get; set; }
        public string Action { get; set; }
        public int CustomerId { get; set; }
        public int EntityId { get; set; }
        public string Value { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}