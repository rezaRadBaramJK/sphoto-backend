using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.Seo
{
    /// <summary>
    ///     Represents an URL record
    /// </summary>
    public class UrlRecordDto : DtoWithId
    {
        /// <summary>
        ///     Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        ///     Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        ///     Gets or sets the slug
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        ///     Gets or sets the value indicating whether the record is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }
    }
}