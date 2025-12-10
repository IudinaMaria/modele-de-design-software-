using Microsoft.Extensions.Options;

public sealed class HttpReaderFactory : IReaderFactory
{
    private readonly IOptionsSnapshot<HttpReaderOptions> _opt;
    private readonly IHttpClientFactory _http;

    public HttpReaderFactory(
        IOptionsSnapshot<HttpReaderOptions> opt,
        IHttpClientFactory http)
    {
        _opt = opt;
        _http = http;
    }

    public IReader Create()
    {
        var o = _opt.Value;
        var client = _http.CreateClient();

        if (o.Format is null)
            throw new InvalidOperationException("Format required.");

        return new HttpReader(o, client);
    }
}
