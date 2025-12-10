public interface ITransformationStep<T>
{
    IEnumerable<T> Apply(IEnumerable<T> items);
}
