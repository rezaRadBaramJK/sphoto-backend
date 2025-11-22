using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Exceptions;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianCustomerRoleService
    {
        /// <exception cref="CustomerRoleNotFoundException"></exception>
        Task<int> GetRegisterCustomerRoleAsync();

        Task AddTechnicianIfNotExistAsync();

        /// <exception cref="CustomerRoleNotFoundException"></exception>
        Task<int> GetTechnicianRoleIdAsync();
    }
}