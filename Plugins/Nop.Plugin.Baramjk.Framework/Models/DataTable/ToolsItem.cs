namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public class ToolsItem
    {
        public string ElementType { get; set; }
        public string ClassName { get; set; }
        public string Href { get; set; }
        public string Content { get; set; }
        public string Icon { get; set; }
        public string OnClick { get; set; }
        public string Attributes { get; set; }
    }

    public class LinkButton : ToolsItem
    {
        public LinkButton(string title)
        {
            ElementType = "a";
            Content = title;
        }
    }
}