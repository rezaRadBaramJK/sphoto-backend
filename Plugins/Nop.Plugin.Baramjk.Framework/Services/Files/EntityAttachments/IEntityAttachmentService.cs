using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments
{
    public interface IEntityAttachmentService
    {
        Task<List<IEntityAttachmentModel>> GetAttachmentsAsync(string entityName = null, int? entityId = null,
            AttachmentType? attachmentType = null, string tag = "", bool includeExpired = false);
    }
}