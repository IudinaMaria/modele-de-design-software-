public sealed class HttpReader : IReader
{
    private readonly HttpReaderOptions _o;
    private readonly HttpClient _http;

    public HttpReader(HttpReaderOptions o, HttpClient http)
    {
        _o = o;
        _http = http;
    }

    public async Task<IEnumerable<T>> Read<T>()
    {
        var stream = await _http.GetStreamAsync(_o.Url);
        return await _o.Format!.Read<T>(stream);
    }
}
