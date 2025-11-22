using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables
{
    public class DataTablesBuilders : IDataTablesBuilders
    {
        private static readonly Dictionary<Type, DataTablesModel> _dataTablesModels = new();

        private readonly ILocalizationService _localizationService;

        public DataTablesBuilders(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public virtual async Task<DataTablesModel> BuildDataTablesModelAsync(DefaultListViewModel model)
        {
            var dataTablesModel = await BuildDataTablesModelAsync(model.ModelType, model.ControllerName, model.Length,
                model.ReadAction, model.DeleteAction, model.UpdateAction, model.LengthMenu);

            if (model.FilterItems != null)
            {
                dataTablesModel.Filters = model.FilterItems.Select(i => i.Type == null
                    ? new FilterParameter(i.Name, value: i.Value)
                    : new FilterParameter(i.Name, i.Type)).ToList();
            }

            return dataTablesModel;
        }

        public virtual async Task<DataTablesModel> BuildDataTablesModelAsync<T>(
            string controllerName,
            int length = 5,
            string readAction = "List",
            string deleteAction = "Delete",
            string updateAction = "Edit",
            string lengthMenu = "5,10,15,20,100"
        )
        {
            var type = typeof(T);
            return await BuildDataTablesModelAsync(type, controllerName, length, readAction, deleteAction, updateAction,
                lengthMenu);
        }


        public virtual async Task<DataTablesModel> BuildDataTablesModelAsync(Type type,
            string controllerName,
            int length = 5,
            string readAction = "List",
            string deleteAction = "Delete",
            string updateAction = "Edit",
            string lengthMenu = "5,10,15,20,100"
        )
        {
            controllerName ??= type.Name;

            var columnProperties = new List<ColumnProperty>
            {
                new("Id")
                {
                    IsMasterCheckBox = true,
                    Render = new RenderCheckBox("checkbox_products"),
                    ClassName = NopColumnClassDefaults.CenterAll,
                    Width = "50",
                    Visible = false
                }
            };

            foreach (var info in type.GetProperties())
            {
                string title;

                if (info.GetCustomAttributes(typeof(NopResourceDisplayNameAttribute), true)
                        .FirstOrDefault() is NopResourceDisplayNameAttribute nopRes)
                {
                    title = await _localizationService.GetResourceAsync(nopRes.ResourceKey);
                }
                else if (info.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                             .FirstOrDefault() is DisplayNameAttribute displayNameAttribute)
                {
                    title = await _localizationService.GetResourceAsync(displayNameAttribute.DisplayName);
                }
                else
                    title = info.Name;


                var columnProperty = new ColumnProperty(info.Name)
                {
                    Title = title
                };

                if (info.GetCustomAttributes(typeof(TableFieldAttribute), true)
                        .FirstOrDefault() is TableFieldAttribute attribute)
                {
                    if (attribute.Title.HasValue())
                    {
                        title = attribute.Title;
                        columnProperty.Title = attribute.Title;
                    }

                    if (attribute.RenderCustom.HasValue())
                        columnProperty.Render = new RenderCustom(attribute.RenderCustom);
                    else if (attribute.IsRenderButtonView)
                        columnProperty.Render =
                            new RenderButtonView(new DataUrl(url: attribute.Url, dataId: attribute.DataId));
                    else if (attribute.IsRenderPicture)
                        columnProperty.Render = new RenderPicture();
                    else if (attribute.IsRenderButtonCustom)
                    {
                        if (attribute.Url.HasValue())
                            columnProperty.Render = new RenderButtonCustom(attribute.ClassName, title)
                            {
                                Url = attribute.Url
                            };
                        else
                            columnProperty.Render = new RenderButtonCustom(attribute.ClassName, title)
                            {
                                OnClickFunctionName = attribute.OnClickFunctionName
                            };
                    }

                    columnProperty.Width = attribute.Width;
                    columnProperty.ClassName = attribute.IsRenderButtonCustom 
                        ? NopColumnClassDefaults.Button 
                        :  attribute.ClassName;
                    
                    columnProperty.Visible = attribute.Visible;
                }

                columnProperties.Add(columnProperty);
            }

            if (string.IsNullOrEmpty(updateAction) == false)
            {
                columnProperties.Add(new ColumnProperty("Id")
                {
                    Title = await _localizationService.GetResourceAsync("Admin.Common.Edit"),
                    ClassName = NopColumnClassDefaults.Button,
                    Width = "100",
                    Render = new RenderButtonEdit(new DataUrl($"~/Admin/{controllerName}/{updateAction}", "Id"))
                });
            }

            if (string.IsNullOrEmpty(deleteAction) == false)
            {
                columnProperties.Add(new ColumnProperty("Id")
                {
                    Title = await _localizationService.GetResourceAsync("Admin.Common.Delete"),
                    Width = "100",
                    ClassName = NopColumnClassDefaults.Button,
                    Render = new RenderButtonRemove("Delete")
                });
            }

            var model = new DataTablesModel
            {
                Name = type.Name + "Table",
                UrlRead = new DataUrl(readAction, controllerName, null),
                UrlDelete = new DataUrl(deleteAction, controllerName, null),
                UrlUpdate = new DataUrl(updateAction, controllerName, null),
                LengthMenu = lengthMenu,
                ColumnCollection = columnProperties
            };

            _dataTablesModels[type] = model;

            return model;
        }
    }
}