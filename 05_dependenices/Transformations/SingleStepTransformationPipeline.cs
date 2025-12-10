public sealed class SingleStepTransformationPipeline : IItemTransformationPipeline
{
    private readonly PriceLargerThanAdapter _step;

    public SingleStepTransformationPipeline(PriceLargerThanAdapter step)
    {
        _step = step;
    }

    public IEnumerable<Item> Run(IEnumerable<Item> items)
        => _step.Apply(items);
}
