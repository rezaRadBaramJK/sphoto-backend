using System;

namespace Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables
{
    public class TableFieldAttribute : Attribute
    {
        public string Title { get; set; }
        public string RenderCustom { get; set; }
        public string Width { get; set; }
        public string ClassName { get; set; }
        public bool Visible { get; set; } = true;
        public string Url { get; set; }
        public string DataId { get; set; }
        public string OnClickFunctionName { get; set; }
        public bool IsRenderButtonView { get; set; }
        public bool IsRenderButtonCustom { get; set; }
        public bool IsRenderPicture { get; set; }
    }

    public class RenderButtonViewAttribute : TableFieldAttribute
    {
        public RenderButtonViewAttribute(string url, string dataId = "Id")
        {
            Url = url;
            DataId = dataId;
            IsRenderButtonView = true;
        }
    }

    public class RenderButtonCustomUrlAttribute : TableFieldAttribute
    {
        public RenderButtonCustomUrlAttribute(string url)
        {
            Url = url;
            IsRenderButtonCustom = true;
        }
    }

    public class RenderButtonCustomFunctionAttribute : TableFieldAttribute
    {
        public RenderButtonCustomFunctionAttribute(string onClickFunctionName)
        {
            OnClickFunctionName = onClickFunctionName;
            IsRenderButtonCustom = true;
        }
    }

    public class RenderPictureAttribute : TableFieldAttribute
    {
        public RenderPictureAttribute()
        {
            IsRenderPicture = true;
        }
    }
}