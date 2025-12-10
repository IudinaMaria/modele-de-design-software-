using Microsoft.Extensions.Options;

public sealed class FileWriterFactory : IWriterFactory
{
    private readonly IOptionsSnapshot<FormatFileOptions> _opt;

    public FileWriterFactory(IOptionsSnapshot<FormatFileOptions> opt)
    {
        _opt = opt;
    }

    public IWriter Create()
    {
        var o = _opt.Get(FormatFileOptions.WriterKey);
        return new FileWriter(o.FileName, o.Format);
    }
}
