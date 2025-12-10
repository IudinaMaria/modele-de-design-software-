using System.Xml;
using System.Xml.Serialization;

public sealed class XmlWriter : IWriter
{
    private readonly string _file;

    public XmlWriter(string file)
    {
        _file = file;
    }

    public Task Write<T>(IEnumerable<T> values)
    {
        var serializer = new XmlSerializer(typeof(List<T>));
        await using var file = new FileStream(_file, FileMode.Create);
        serializer.Serialize(file, values.ToList());
        return Task.CompletedTask;
    }
}
