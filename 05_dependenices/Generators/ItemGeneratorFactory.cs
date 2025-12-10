public sealed class ItemGeneratorFactory : IObjectGeneratorFactory
{
    private readonly IServiceProvider _sp;

    public ItemGeneratorFactory(IServiceProvider sp)
    {
        _sp = sp;
    }

    public IObjectGenerator<T> Create<T>()
        => _sp.GetRequiredService<IObjectGenerator<T>>();
}
