public sealed class PriceLargerThanAdapter : ITransformationStep<Item>
{
    private readonly PriceLargerThanConfig _cfg;

    public PriceLargerThanAdapter(PriceLargerThanConfig cfg)
    {
        _cfg = cfg;
    }

    public IEnumerable<Item> Apply(IEnumerable<Item> items)
        => Processing.PriceLargerThan(items, _cfg);
}
