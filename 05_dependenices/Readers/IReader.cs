public interface IReader
{
    Task<IEnumerable<T>> Read<T>();
}
