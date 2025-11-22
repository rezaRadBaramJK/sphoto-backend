namespace Nop.Plugin.Baramjk.Framework.Events
{
    public class BaramjkEvent
    {
        public string Name { get; }
    }

    public class BaramjkEvent<T>
    {
        public BaramjkEvent(T entity)
        {
            Entity = entity;
            Name = typeof(T).Name;
        }

        public BaramjkEvent(T entity, string name)
        {
            Entity = entity;
            Name = name;
        }

        public T Entity { get; }

        public string Name { get; }
    }
}