using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class OrchestrationBuilderExtensions
{
    public static OrchestrationBuilder CreateReadWritePipelineBuilder(this IServiceScope scope)
        => new() { ServiceProvider = scope.ServiceProvider };

    public static void UseFileReader(this OrchestrationBuilder b, Action<FormatFileOptions> configure)
    {
        b.Model.Reader = ActivatorUtilities.GetServiceOrCreateInstance<FileReaderFactory>(b.ServiceProvider);
        var opts = b.ServiceProvider.GetRequiredService<IOptionsSnapshot<FormatFileOptions>>()
            .Get(FormatFileOptions.ReaderKey);
        configure(opts);
    }

    public static void UseRandomReader(this OrchestrationBuilder b, Action<RandomReaderFactoryOptions> configure)
    {
        b.Model.Reader = ActivatorUtilities.GetServiceOrCreateInstance<RandomReaderFactory>(b.ServiceProvider);
        configure(b.ServiceProvider.GetRequiredService<IOptionsSnapshot<RandomReaderFactoryOptions>>().Value);
    }

    public static void UseHttpReader(this OrchestrationBuilder b, Action<HttpReaderOptions> configure)
    {
        b.Model.Reader = ActivatorUtilities.GetServiceOrCreateInstance<HttpReaderFactory>(b.ServiceProvider);
        configure(b.ServiceProvider.GetRequiredService<IOptionsSnapshot<HttpReaderOptions>>().Value);
    }

    public static void UseFileWriter(this OrchestrationBuilder b, Action<FormatFileOptions> configure)
    {
        b.Model.Writer = ActivatorUtilities.GetServiceOrCreateInstance<FileWriterFactory>(b.ServiceProvider);
        var opts = b.ServiceProvider.GetRequiredService<IOptionsSnapshot<FormatFileOptions>>()
            .Get(FormatFileOptions.WriterKey);
        configure(opts);
    }

    public static void UseXmlWriter(this OrchestrationBuilder b, Action<FormatFileOptions> configure)
    {
        b.Model.Writer = ActivatorUtilities.GetServiceOrCreateInstance<XmlWriterFactory>(b.ServiceProvider);
        var opts = b.ServiceProvider.GetRequiredService<IOptionsSnapshot<FormatFileOptions>>()
            .Get(FormatFileOptions.WriterKey);
        configure(opts);
    }

    public static void UseLoggingWriter(this OrchestrationBuilder b)
        => b.Model.Writer = new LoggingWriterFactory();

    public static void UsePipeline<T>(this OrchestrationBuilder b)
        where T : IItemTransformationPipeline
        => b.Model.Transform = b.ServiceProvider.GetRequiredService<T>();
}
