using Microsoft.Extensions.Options;

public sealed class RandomReaderFactory : IReaderFactory
{
    private readonly IOptionsSnapshot<RandomReaderFactoryOptions> _opt;
    private readonly IObjectGeneratorFactory _gen;

    public RandomReaderFactory(
        IOptionsSnapshot<RandomReaderFactoryOptions> opt,
        IObjectGeneratorFactory gen)
    {
        _opt = opt;
        _gen = gen;
    }

    public IReader Create()
    {
        return new RandomReader(_opt.Value.GeneratedCount, _gen);
    }
}
