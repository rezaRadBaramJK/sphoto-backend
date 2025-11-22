using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.OtpAuthentication.Consts;
using Nop.Plugin.Baramjk.OtpAuthentication.Domain;
using Nop.Plugin.Baramjk.OtpAuthentication.Services.Abstractions;

namespace Nop.Plugin.Baramjk.OtpAuthentication.Services
{
    public class MobileOtpService : IMobileOtpService
    {
        private readonly IRepository<MobileOtp> _repository;

        public MobileOtpService(IRepository<MobileOtp> repository)
        {
            _repository = repository;
        }

        public async Task InsertAsync(MobileOtp entity)
        {
            await _repository.InsertAsync(entity);
        }

        public async Task UpdateAsync(MobileOtp entity)
        {
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(MobileOtp entity)
        {
            await _repository.DeleteAsync(entity);
        }

        public async Task<MobileOtp> FindByPhoneNumber(string phoneNumber, OtpType otpType)
        {
            return await _repository.Table.FirstOrDefaultAsync(x => x.PhoneNumber.Equals(phoneNumber));
        }
    }
}