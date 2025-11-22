using System;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baramjk.Framework.Models.DataTable
{
    public class ExtendedSearchModel : IPagingRequestModel
    {
        public ExtendedSearchModel()
        {
            SetGridPageSize();
        }

        #region Properties

        public int Page => Start / Length + 1;
        public int PageSize => Length;
        public string AvailablePageSizes { get; set; }
        public int Draw { get; set; }
        public int Start { get; set; }

        private int _length;

        public int Length
        {
            get => Math.Max(_length, 1);
            set => _length = value;
        }

        public int NextDraw => Draw + 1;

        #endregion

        #region Methods

        public void SetGridPageSize()
        {
            var adminAreaSettings = EngineContext.Current.Resolve<AdminAreaSettings>();
            SetGridPageSize(adminAreaSettings?.DefaultGridPageSize ?? 0, adminAreaSettings?.GridPageSizes);
        }

        public void SetPopupGridPageSize()
        {
            var adminAreaSettings = EngineContext.Current.Resolve<AdminAreaSettings>();
            SetGridPageSize(adminAreaSettings.PopupGridPageSize, adminAreaSettings.GridPageSizes);
        }

        public void SetGridPageSize(int pageSize, string availablePageSizes = null)
        {
            Start = 0;
            Length = pageSize;
            AvailablePageSizes = availablePageSizes;
        }

        #endregion
    }
}