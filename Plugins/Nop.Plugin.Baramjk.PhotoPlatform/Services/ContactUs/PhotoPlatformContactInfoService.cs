using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs
{
    public class PhotoPlatformContactInfoService
    {
        private readonly IRepository<ContactInfoEntity> _contactInfoRepository;

        public PhotoPlatformContactInfoService(IRepository<ContactInfoEntity> contactInfoRepository)
        {
            _contactInfoRepository = contactInfoRepository;
        }

        public Task InsertAsync(ContactInfoEntity entityToInsert)
        {
            return _contactInfoRepository.InsertAsync(entityToInsert);
        }

        public Task UpdateAsync(ContactInfoEntity entityToUpdate)
        {
            return _contactInfoRepository.UpdateAsync(entityToUpdate);
        }

        public Task DeleteAsync(ContactInfoEntity entityToDelete)
        {
            return _contactInfoRepository.DeleteAsync(entityToDelete);
        }

        public Task<ContactInfoEntity> GetByIdAsync(int id)
        {
            return _contactInfoRepository.GetByIdAsync(id);
        }

        public Task<List<ContactInfoEntity>> GetAsync(string email, string phoneNumber)
        {
            return _contactInfoRepository.Table
                .Where(ci => ci.Email == email && ci.PhoneNumber == phoneNumber)
                .ToListAsync();
        }

        public Task<IPagedList<ContactInfoEntity>> GetAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            return 
            _contactInfoRepository.Table
                .OrderByDescending(entity => entity.Id)
                .ToPagedListAsync(pageIndex, pageSize);
        }
        
        public async Task DeleteByIdAsync(int id)
        {
            var entityToDelete = await _contactInfoRepository.GetByIdAsync(id);
            if(entityToDelete == null)
                return;
            await _contactInfoRepository.DeleteAsync(entityToDelete);
        }

        public Task MarkAsPaidAsync(ContactInfoEntity contactInfo)
        {
            contactInfo.HasBeenPaid = true;
            contactInfo.PaymentUtcDateTime = DateTime.UtcNow;
            return _contactInfoRepository.UpdateAsync(contactInfo);
        }
    }
}