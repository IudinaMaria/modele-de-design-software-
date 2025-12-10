public sealed class ListItemTransformationPipeline : IItemTransformationPipeline
{
    private readonly IEnumerable<ITransformationStep<Item>> _steps;

    public ListItemTransformationPipeline(IEnumerable<ITransformationStep<Item>> steps)
    {
        _steps = steps;
    }

    public IEnumerable<Item> Run(IEnumerable<Item> items)
    {
        foreach (var st in _steps)
            items = st.Apply(items);
        return items;
    }
}
