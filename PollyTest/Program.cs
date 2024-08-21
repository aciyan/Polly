using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using PollyTest;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var client = httpClientFactory.CreateClient("RetryClient");

        var payload = new TaxDataReadContext()
        {
            BindingContext = new BindingContext()
            {
                ReturnId = "d8551890-af97-4b0e-b03e-93736d1a1922",
                TaxYear = "2023",
                ApplicationId = "TAXPREP",
                FormName = "taxreturnversioninfo",
                TaxSystem = "1040"
            },
            UseSnapshotTom = false
        };
        var jsonPayload = JsonConvert.SerializeObject(payload);
        var request = new PostRequestContext { Url = "https://localhost:44342/tax/data/read", Payload = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json") };
        //var handler = serviceProvider.GetRequiredService<HttpRequestHandler>();
        //var result = await handler.HandlePostRequest(request, client);
        var result = await client.PostAsync(request.Url, request.Payload).ConfigureAwait(false);

        Console.WriteLine($"Response status code: {result.StatusCode}");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient("RetryClient")
            .AddPolicyHandler(PrintResiliencePipelineBuilder.BuildResiliencePipeline().Build().AsAsyncPolicy<HttpResponseMessage>());
        services.AddHttpClient("RetryClient")
            .AddPolicyHandler(PrintResiliencePipelineBuilder.GetRetryPolicy()) ;
        //services.AddResiliencePipeline("SampleResiliencePipeline", builder => PrintResiliencePipelineBuilder.BuildResiliencePipeline());
        services.AddSingleton<HttpRequestHandler>();
    }
}