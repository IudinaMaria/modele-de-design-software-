public static class Orchestration
{
    public static async Task WholeProcess(
        IReader reader,
        IWriter writer,
        IItemTransformationPipeline pipeline)
    {
        var items = await reader.Read<Item>();
        var proc = pipeline.Run(items);
        await writer.Write(proc);
    }
}
