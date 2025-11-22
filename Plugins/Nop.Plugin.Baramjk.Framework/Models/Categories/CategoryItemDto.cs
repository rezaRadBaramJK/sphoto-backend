namespace Nop.Plugin.Baramjk.Framework.Models.Categories
{
    public record CategoryItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual bool Equals(CategoryItemDto other)
        {
            return other != null && Id == other.Id;
        }
    }
}