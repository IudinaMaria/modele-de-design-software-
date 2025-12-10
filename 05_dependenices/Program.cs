using System.Globalization;
using System.Text.Json;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var services = new ServiceCollection();

// Options
services.AddOptions<FormatFileOptions>(FormatFileOptions.ReaderKey);
services.AddOptions<FormatFileOptions>(FormatFileOptions.WriterKey);
services.AddOptions<HttpReaderOptions>();
services.AddOptions<RandomReaderFactoryOptions>();

// HttpClient
services.AddHttpClient();

// Generators
services.AddSingleton<IObjectGenerator<Item>, ItemGenerator>();
services.AddSingleton<IObjectGeneratorFactory, ItemGeneratorFactory>();

// Readers
services.AddSingleton<FileReaderFactory>();
services.AddSingleton<RandomReaderFactory>();
services.AddSingleton<HttpReaderFactory>();

// Writers
services.AddSingleton<FileWriterFactory>();
services.AddSingleton<LoggingWriterFactory>();
services.AddSingleton<XmlWriterFactory>();

// Transformers
services.AddSingleton<IItemTransformer>(new PrefixItemTransformer("my-prefix-"));

services.AddSingleton<ITransformationStep<Item>>(sp =>
{
    return new PriceLargerThanAdapter(new PriceLargerThanConfig
    {
        PriceThreshold = 100
    });
});

services.AddSingleton<ITransformationStep<Item>>(sp =>
{
    return new ChangeByNameLookupAdapter(new ChangeNameByLookupConfig
    {
        Map = new()
        {
            ["Item1"] = "RenamedItem",
        }
    });
});

// Complex
services.AddSingleton<ITransformationStep<Item>>(sp =>
{
    var cfg = new ComplexTransformationConfig
    {
        Transformers = sp.GetServices<IItemTransformer>().ToList()
    };
    return new ComplexTransformationAdapter(cfg);
});

// Pipelines
services.AddSingleton<SingleStepTransformationPipeline>();
services.AddSingleton<ListItemTransformationPipeline>();

await using var provider = services.BuildServiceProvider();


// EXAMPLES OF RUNNING

await Run(builder =>
{
    builder.UsePipeline<ListItemTransformationPipeline>();

    builder.UseHttpReader(opts =>
    {
        opts.Url = "https://example.com/items.json";
        opts.Format = Format.CreateJson(new JsonSerializerOptions());
    });

    builder.UseFileWriter(opts =>
    {
        opts.Format = Format.CreateXml();
        opts.FileName = "output.xml";
    });
});

await Run(builder =>
{
    builder.UsePipeline<SingleStepTransformationPipeline>();

    builder.UseRandomReader(opts =>
    {
        opts.GeneratedCount = 5;
    });

    builder.UseFileWriter(opts =>
    {
        opts.Format = Format.CreateCsv(new CsvConfiguration(CultureInfo.InvariantCulture));
        opts.FileName = "output.csv";
    });
});


async Task Run(Action<OrchestrationBuilder> configure)
{
    await using var scope = provider.CreateAsyncScope();
    var builder = scope.CreateReadWritePipelineBuilder();
    configure(builder);
    var exec = builder.Build();
    await exec.Execute();
}
