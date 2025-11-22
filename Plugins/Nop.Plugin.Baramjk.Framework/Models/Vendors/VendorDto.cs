using System.Collections.Generic;
using Nop.Web.Models.Media;
using Nop.Web.Models.Vendors;

namespace Nop.Plugin.Baramjk.Framework.Models.Vendors
{
    public class VendorDto : DtoBase
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public PictureModel Picture { get; set; }

        public int Rating { get; set; }

        public int FavCount { get; set; }

        public bool IsFavourite { get; set; }

        public IList<VendorAttributeModel> VendorAttributes { get; set; }
        public List<KeyValuePair<string, string>> GenericAttributes { get; set; }
    }
}