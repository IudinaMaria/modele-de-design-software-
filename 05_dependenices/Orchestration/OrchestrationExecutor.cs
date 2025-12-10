public readonly struct OrchestrationExecutor
{
    private readonly IReader _reader;
    private readonly IWriter _writer;
    private readonly IItemTransformationPipeline _pipeline;

    public OrchestrationExecutor(
        IReader reader,
        IWriter writer,
        IItemTransformationPipeline pipeline)
    {
        _reader = reader;
        _writer = writer;
        _pipeline = pipeline;
    }

    public Task Execute()
        => Orchestration.WholeProcess(_reader, _writer, _pipeline);
}
