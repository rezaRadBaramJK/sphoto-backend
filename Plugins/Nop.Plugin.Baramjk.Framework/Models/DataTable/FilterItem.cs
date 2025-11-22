using System;

namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public class FilterItem
    {
        public FilterItem(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public FilterItem(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public string ModelName { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
        public bool IsHiddenField { get; set; }
    }
}