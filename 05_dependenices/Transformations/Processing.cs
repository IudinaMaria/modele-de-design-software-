public static class Processing
{
    public static IEnumerable<Item> ChangeNameByLookup(
        IEnumerable<Item> items,
        ChangeNameByLookupConfig cfg)
    {
        foreach (var i in items)
        {
            if (cfg.Map.TryGetValue(i.Name, out var newName))
                i.Name = newName;
            yield return i;
        }
    }

    public static IEnumerable<Item> PriceLargerThan(
        IEnumerable<Item> items,
        PriceLargerThanConfig cfg)
    {
        foreach (var it in items)
            if (it.Price > cfg.PriceThreshold)
                yield return it;
    }

    public static IEnumerable<Item> ComplexTransformation(
        IEnumerable<Item> items,
        ComplexTransformationConfig cfg)
    {
        foreach (var it in items)
        {
            var t = it;
            foreach (var tr in cfg.Transformers)
                t = tr.Transform(t);
            yield return t;
        }
    }
}
