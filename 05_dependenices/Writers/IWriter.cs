public interface IWriter
{
    Task Write<T>(IEnumerable<T> values);
}
