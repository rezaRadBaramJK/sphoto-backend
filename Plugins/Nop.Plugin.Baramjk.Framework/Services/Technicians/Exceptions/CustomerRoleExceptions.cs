using System;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Exceptions
{
    public class CustomerRoleNotFoundException : Exception
    {
        public CustomerRoleNotFoundException(string name) : base($"{name} customer role not found.")
        {
            
        }
    }
}