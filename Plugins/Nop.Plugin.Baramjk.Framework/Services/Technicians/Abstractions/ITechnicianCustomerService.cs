using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianCustomerService
    {
        Task<List<Customer>> GetNotTechniciansRegisteredCustomersAsync();
        Task DeleteCustomerRoleMappingAsync(int customerId);
        Task<CustomerRegistrationServiceResult> RegisterAsync(RegisterCustomerServiceParams serviceParams);
        Task RemoveTechniciansCustomerRolesAsync();
        Task AddCustomerRoleMappingAsync(Customer customer);

    }
}