public sealed class LoggingWriter : IWriter
{
    public Task Write<T>(IEnumerable<T> values)
    {
        foreach (var x in values)
            Console.WriteLine(x);
        return Task.CompletedTask;
    }
}
