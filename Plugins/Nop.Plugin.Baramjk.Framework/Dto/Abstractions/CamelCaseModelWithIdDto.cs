namespace Nop.Plugin.Baramjk.Framework.Dto.Abstractions
{
    
    public abstract class CamelCaseModelWithIdDto: CamelCaseModelDto
    {
        public int Id { get; set; }
    }
}