using System;

namespace Nop.Plugin.Baramjk.Framework.Services.Files.EntityAttachments
{
    public interface IEntityAttachmentModel
    {
        int Id { get; }
        string EntityName { get; }
        int EntityId { get; }
        string FileName { get; }
        string Title { get; }
        string Text { get; }
        string Link { get; }
        string AltText { get; }
        string Tag { get; }
        int DisplayOrder { get; }
        AttachmentType AttachmentType { get; }
        string DownloadUrl { get; }
        DateTime? ExpirationDateTime { get; set; }
    }
}