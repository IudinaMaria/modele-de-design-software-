public sealed class RandomReader : IReader
{
    private readonly int _count;
    private readonly IObjectGeneratorFactory _gen;

    public RandomReader(int count, IObjectGeneratorFactory gen)
    {
        _count = count;
        _gen = gen;
    }

    public Task<IEnumerable<T>> Read<T>()
    {
        var g = _gen.Create<T>();
        return Task.FromResult(g.Generate(_count));
    }
}
