using Microsoft.Extensions.Options;

public sealed class XmlWriterFactory : IWriterFactory
{
    private readonly IOptionsSnapshot<FormatFileOptions> _opt;

    public XmlWriterFactory(IOptionsSnapshot<FormatFileOptions> opt)
    {
        _opt = opt;
    }

    public IWriter Create()
    {
        var o = _opt.Get(FormatFileOptions.WriterKey);

        if (o.Format.Type != FormatType.Xml)
            throw new InvalidOperationException("Format must be XML");

        return new XmlWriter(o.FileName);
    }
}
