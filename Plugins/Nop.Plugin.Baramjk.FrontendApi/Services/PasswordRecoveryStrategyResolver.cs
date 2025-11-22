using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Baramjk.FrontendApi.Models.PasswordRecoveries;
using Nop.Plugin.Baramjk.FrontendApi.Services.Abstractions;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class PasswordRecoveryStrategyResolver
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public PasswordRecoveryStrategyResolver(
            IEnumerable<IPasswordRecoveryService> passwordRecoveryServices,
            FrontendApiSettings frontendApiSettings)
        {
            _passwordRecoveryService = passwordRecoveryServices.FirstOrDefault(s => s.PasswordRecoveryStrategy == frontendApiSettings.PasswordRecoveryStrategy);
        }


        public Task<PasswordRecoverySendResult> Send(PasswordRecoveryModel model)
        {
            return _passwordRecoveryService.Send(model);
        }
    }
}