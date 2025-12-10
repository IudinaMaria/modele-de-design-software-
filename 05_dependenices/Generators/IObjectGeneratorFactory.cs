public interface IObjectGeneratorFactory
{
    IObjectGenerator<T> Create<T>();
}
