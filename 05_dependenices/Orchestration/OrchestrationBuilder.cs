public sealed class OrchestrationBuilderModel
{
    public IReaderFactory? Reader { get; set; }
    public IWriterFactory? Writer { get; set; }
    public IItemTransformationPipeline? Transform { get; set; }
}

public sealed class OrchestrationBuilder
{
    public required IServiceProvider ServiceProvider { get; init; }
    public OrchestrationBuilderModel Model { get; } = new();

    public OrchestrationExecutor Build()
    {
        if (Model.Reader is null) throw new InvalidOperationException();
        if (Model.Writer is null) throw new InvalidOperationException();

        var reader = Model.Reader.Create();
        var writer = Model.Writer.Create();
        var pipeline = Model.Transform ?? DoNothingItemTransformationPipeline.Instance;

        return new OrchestrationExecutor(reader, writer, pipeline);
    }
}
