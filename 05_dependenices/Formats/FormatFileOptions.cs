public sealed class FormatFileOptions
{
    public const string ReaderKey = "Reader";
    public const string WriterKey = "Writer";

    public string FileName { get; set; } = string.Empty;
    public required Format Format { get; set; }
}
