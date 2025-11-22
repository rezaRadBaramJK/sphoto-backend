using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public class DefaultListViewModel
    {
        public DefaultListViewModel(string controllerName, string systemName, Type modelType)
        {
            ControllerName = controllerName;
            PageTitle = controllerName;
            SystemName = systemName;
            ModelType = modelType;
        }

        public Type ModelType { get; set; }
        public List<FilterItem> FilterItems { get; set; }
        public List<ToolsItem> Tools { get; set; }
        public string SystemName { get; set; }
        public string PageTitle { get; set; }
        public string ControllerName { get; set; }
        public string ViewFullPath { get; set; } = FrameworkDefaultValues.DefaultListViewPath;
        public string ReadAction { get; set; } = "List";
        public string DeleteAction { get; set; } = "Delete";
        public string UpdateAction { get; set; } = "Edit";
        public string AddAction { get; set; } = "Add";
        public string JsPath { get; set; }
        public string CssPath { get; set; }
        public string Javascript { get; set; }
        public int Length { get; set; } = 5;
        public string LengthMenu { get; set; } = "5,10,15,20,100";
        public DataTablesModel DataTablesModel { get; set; }
        public string ActiveMenuSystemName { get; set; } = null;

        public string BeforePartialViewName { get; set; } = null;
        public string AfterPartialViewName { get; set; } = null;

        public string BeforePartialViewData { get; set; } = null;
        public string AfterPartialViewData { get; set; } = null;
    }
}