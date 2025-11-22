using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services.ContactUs
{
    public class PhotoPlatformSubjectService
    {
        private readonly IRepository<SubjectEntity> _subjectRepository;

        public PhotoPlatformSubjectService(IRepository<SubjectEntity> subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public Task InsertAsync(SubjectEntity entity)
        {
            return _subjectRepository.InsertAsync(entity);
        }

        public Task UpdateAsync(SubjectEntity entityToUpdate)
        {
            return _subjectRepository.UpdateAsync(entityToUpdate);
        }

        public Task DeleteAsync(SubjectEntity entityToDelete)
        {
            return _subjectRepository.DeleteAsync(entityToDelete);
        }

        public async Task DeleteAsync(int id)
        {
            var entityToDelete = await GetByIdAsync(id);
            if (entityToDelete == null)
                return;

            await DeleteAsync(entityToDelete);
        }

        public Task<SubjectEntity> GetByIdAsync(int id)
        {
            return _subjectRepository.GetByIdAsync(id);
        }

        public Task<IPagedList<SubjectEntity>> GetAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            return _subjectRepository.Table
                .Where(s => showHidden || s.Deleted == false)
                .ToPagedListAsync(pageIndex, pageSize);
        }
    }
}