public sealed class FileWriter : IWriter
{
    private readonly string _file;
    private readonly Format _format;

    public FileWriter(string file, Format format)
    {
        _file = file;
        _format = format;
    }

    public async Task Write<T>(IEnumerable<T> values)
    {
        await using var s = new FileStream(_file, FileMode.Create);
        await _format.Write(s, values);
    }
}
