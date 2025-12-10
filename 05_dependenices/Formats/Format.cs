using System.Diagnostics;
using System.Text.Json;
using System.Xml.Serialization;
using CsvHelper.Configuration;

public sealed class Format
{
    private Format(FormatType type)
    {
        Type = type;
    }

    public FormatType Type { get; }
    public JsonSerializerOptions? JsonOptions { get; private init; }
    public CsvConfiguration? CsvConfig { get; private init; }

    public static Format CreateJson(JsonSerializerOptions opts)
        => new(FormatType.Json) { JsonOptions = opts };

    public static Format CreateCsv(CsvConfiguration config)
        => new(FormatType.Csv) { CsvConfig = config };

    public static Format CreateText()
        => new(FormatType.Text);

    public static Format CreateXml()
        => new(FormatType.Xml);

    public async Task<IEnumerable<T>> Read<T>(Stream s)
    {
        return Type switch
        {
            FormatType.Json => await JsonSerializer.DeserializeAsync<IEnumerable<T>>(s, JsonOptions!) 
                               ?? Array.Empty<T>(),
            FormatType.Csv => ReadCsv<T>(s),
            FormatType.Xml => throw new NotSupportedException("XML reading unsupported."),
            _ => throw new NotSupportedException(),
        };
    }

    private IEnumerable<T> ReadCsv<T>(Stream s)
    {
        using var reader = new StreamReader(s);
        using var csv = new CsvHelper.CsvReader(reader, CsvConfig!);
        return csv.GetRecords<T>().ToList();
    }

    public async Task Write<T>(Stream s, IEnumerable<T> items)
    {
        switch (Type)
        {
            case FormatType.Json:
                await JsonSerializer.SerializeAsync(s, items, JsonOptions);
                break;

            case FormatType.Csv:
                await WriteCsv(s, items);
                break;

            case FormatType.Text:
                using (var sw = new StreamWriter(s))
                    foreach (var i in items)
                        sw.WriteLine(i);
                break;

            case FormatType.Xml:
                var serializer = new XmlSerializer(typeof(List<T>));
                serializer.Serialize(s, items.ToList());
                break;
        }
    }

    private async Task WriteCsv<T>(Stream s, IEnumerable<T> items)
    {
        await using var writer = new StreamWriter(s);
        await using var csv = new CsvHelper.CsvWriter(writer, CsvConfig!);
        await csv.WriteRecordsAsync(items);
    }
}
