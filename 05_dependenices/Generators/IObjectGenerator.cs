public interface IObjectGenerator<T>
{
    IEnumerable<T> Generate(int count);
}
