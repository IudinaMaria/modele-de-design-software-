public sealed class PrefixItemTransformer : IItemTransformer
{
    private readonly string _prefix;

    public PrefixItemTransformer(string prefix)
    {
        _prefix = prefix;
    }

    public Item Transform(Item i)
    {
        i.Name = _prefix + i.Name;
        return i;
    }
}
