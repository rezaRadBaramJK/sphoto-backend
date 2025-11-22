using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class CustomerRegistrationServiceResult
    {
        
        public CustomerRegistrationServiceResult()
        {
            Errors = new List<string>();
        }
        
        public bool Success => !Errors.Any();
        
        public Customer RegisteredCustomer { get; set; }
        
        public void AddError(string error)
        {
            Errors.Add(error);
        }
        
        public IList<string> Errors { get; set; }
    }
}