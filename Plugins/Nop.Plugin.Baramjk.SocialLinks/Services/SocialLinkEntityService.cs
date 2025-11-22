using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.SocialLinks.Domain;
using System.Linq;

namespace Nop.Plugin.Baramjk.SocialLinks.Services
{
    public interface ISocialLinkEntityService
    {
        Task<List<SocialLink>> GetSocialLinks(SocialLinkCategory category);
    }

    public class SocialLinkEntityService : ISocialLinkEntityService
    {
        private readonly IRepository<SocialLink> _repository;

        public SocialLinkEntityService(IRepository<SocialLink> repository)
        {
            _repository = repository;
        }

        public async Task<List<SocialLink>> GetSocialLinks(SocialLinkCategory category)
        {
            try
            {
                var items = _repository.Table
                    .Where(x => x.Category == category)
                    .OrderBy(x => x.Priority)
                    .ToList();
                return items;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }

            return new List<SocialLink>();
        }
    }
}