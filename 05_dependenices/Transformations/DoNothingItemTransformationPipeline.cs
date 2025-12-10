public sealed class DoNothingItemTransformationPipeline : IItemTransformationPipeline
{
    public static readonly DoNothingItemTransformationPipeline Instance = new();

    public IEnumerable<Item> Run(IEnumerable<Item> items) => items;
}
