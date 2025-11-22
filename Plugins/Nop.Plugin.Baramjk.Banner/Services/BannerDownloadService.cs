using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;
using Nop.Data;
using System.Linq;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class BannerDownloadService
    {
        private readonly IRepository<Download> _downloadRepository;

        public BannerDownloadService(IRepository<Download> downloadRepository)
        {
            _downloadRepository = downloadRepository;
        }

        public async Task<IList<Download>> GetDownloadsAsync(IList<Guid> guids)
        {
            var query = from o in _downloadRepository.Table
                where guids.Contains(o.DownloadGuid)
                select o;

            return await query.ToListAsync();
        }

        public async Task AddAsync(IList<Download> downloads)
        {
            await _downloadRepository.InsertAsync(downloads);
        }


    }
}