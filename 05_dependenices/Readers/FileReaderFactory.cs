using Microsoft.Extensions.Options;

public sealed class FileReaderFactory : IReaderFactory
{
    private readonly IOptionsSnapshot<FormatFileOptions> _opt;

    public FileReaderFactory(IOptionsSnapshot<FormatFileOptions> opt)
    {
        _opt = opt;
    }

    public IReader Create()
    {
        var o = _opt.Get(FormatFileOptions.ReaderKey);
        return new FileReader(o.FileName, o.Format);
    }
}
