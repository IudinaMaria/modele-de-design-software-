public interface IItemTransformationPipeline
{
    IEnumerable<Item> Run(IEnumerable<Item> items);
}
