using System;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Models.DataTable;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Baramjk.Framework.Services.Ui.DataTables
{
    public interface IDataTablesBuilders
    {
        Task<DataTablesModel> BuildDataTablesModelAsync<T>(
            string controllerName,
            int length = 5,
            string readAction = "List",
            string deleteAction = "Delete",
            string updateAction = "Edit",
            string lengthMenu = "5,10,15,20,100"
        );

        Task<DataTablesModel> BuildDataTablesModelAsync(Type type,
            string controllerName,
            int length = 5,
            string readAction = "List",
            string deleteAction = "Delete",
            string updateAction = "Edit",
            string lengthMenu = "5,10,15,20,100"
        );

        Task<DataTablesModel> BuildDataTablesModelAsync(DefaultListViewModel model);
    }
}