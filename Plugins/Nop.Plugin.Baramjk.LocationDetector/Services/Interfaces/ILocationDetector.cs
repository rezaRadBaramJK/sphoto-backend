using System.Threading.Tasks;
using Nop.Services.Tasks;

namespace Nop.Plugin.Baramjk.LocationDetector.Services.Interfaces
{
    public interface ILocationDetector
    {
        Task<string> GetLocationByIp(string ip);
    }
}