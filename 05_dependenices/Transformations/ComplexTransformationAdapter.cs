public sealed class ComplexTransformationAdapter : ITransformationStep<Item>
{
    private readonly ComplexTransformationConfig _cfg;

    public ComplexTransformationAdapter(ComplexTransformationConfig cfg)
    {
        _cfg = cfg;
    }

    public IEnumerable<Item> Apply(IEnumerable<Item> items)
        => Processing.ComplexTransformation(items, _cfg);
}
