using System.Threading.Tasks;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Baramjk.Framework.Factories
{
    public interface IPictureModelFactory
    {
        Task<PictureModel> PreparePictureModel(int pictureId, int? productThumbPictureSize = null);
    }
}