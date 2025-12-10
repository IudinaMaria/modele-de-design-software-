public sealed class ChangeByNameLookupAdapter : ITransformationStep<Item>
{
    private readonly ChangeNameByLookupConfig _cfg;

    public ChangeByNameLookupAdapter(ChangeNameByLookupConfig cfg)
    {
        _cfg = cfg;
    }

    public IEnumerable<Item> Apply(IEnumerable<Item> items)
        => Processing.ChangeNameByLookup(items, _cfg);
}
