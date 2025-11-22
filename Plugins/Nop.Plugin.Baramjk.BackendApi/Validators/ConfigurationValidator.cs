using FluentValidation;
using Nop.Plugin.Baramjk.BackendApi.Framework;
using Nop.Plugin.Baramjk.BackendApi.Models;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Baramjk.BackendApi.Validators
{
    /// <summary>
    ///     Represents configuration model validator
    /// </summary>
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator()
        {
            RuleFor(model => model.SecretKey)
                .NotEmpty()
                .MinimumLength(WebApiCommonDefaults.MinSecretKeyLength);
        }

        #endregion
    }
}