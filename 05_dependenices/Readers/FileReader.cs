public sealed class FileReader : IReader
{
    private readonly string _file;
    private readonly Format _format;

    public FileReader(string file, Format format)
    {
        _file = file;
        _format = format;
    }

    public async Task<IEnumerable<T>> Read<T>()
    {
        await using var s = File.OpenRead(_file);
        return await _format.Read<T>(s);
    }
}
