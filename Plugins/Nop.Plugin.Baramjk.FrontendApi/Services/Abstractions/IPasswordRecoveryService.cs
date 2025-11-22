using System.Threading.Tasks;
using Nop.Plugin.Baramjk.FrontendApi.Models.PasswordRecoveries;
using Nop.Plugin.Baramjk.FrontendApi.Models.Types;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Baramjk.FrontendApi.Services.Abstractions
{
    public interface IPasswordRecoveryService
    {

        PasswordRecoveryStrategy PasswordRecoveryStrategy { get; }

        Task<PasswordRecoverySendResult> Send(PasswordRecoveryModel passwordRecoveryModel);
    }
}