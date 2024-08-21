using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Polly;
using Polly.Registry;

namespace PollyTest
{
    public class RequestContextBase
    {
        public string Url { get; set; }
    }

    public class PostRequestContext : RequestContextBase
    {
        public HttpContent Payload { get; set; }
    }

    public class HttpRequestHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public HttpRequestHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<HttpResponseMessage> HandlePostRequest<TRequest>(TRequest request, HttpClient httpClient) where TRequest : PostRequestContext
        {
            var pipeline = _serviceProvider.GetRequiredKeyedService<ResiliencePipeline>("SampleResiliencePipeline");

            return await pipeline.ExecuteAsync(async token =>
            {
                var result = await httpClient.PostAsync(request.Url, request.Payload, token).ConfigureAwait(false);
                return result;
            });
        }
    }
}

