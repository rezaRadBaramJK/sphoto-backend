using System.Linq;

namespace Nop.Plugin.Baramjk.Framework.Services.CleanUps.Abstractions
{
    public interface INopCleanup
    {
        IQueryable<int> GetPictureIdsQuery();
    }
}