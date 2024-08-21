using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyTest
{
    /// <summary>
    /// Create an instance of builder that exposes various extensions for adding resilience strategies
    /// </summary>
    public static class PrintResiliencePipelineBuilder
    {
        #region Public Methods
        /// <summary>
        /// Build the resilience pipeline for retrying
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public static ResiliencePipelineBuilder<HttpResponseMessage> BuildResiliencePipeline()
        {
             var resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                // Specify what exceptions or results should be retried
                ShouldHandle = args => args.Outcome switch
                {
                    { Result: HttpResponseMessage response } when response.StatusCode != System.Net.HttpStatusCode.OK => PredicateResult.True(),
                    { Exception : Exception exceptions } when exceptions is InvalidOperationException => PredicateResult.True(),
                    { Exception: Exception exceptions } when exceptions is HttpRequestException => PredicateResult.True(),
                    _ => PredicateResult.False()
                }, 
                // Specify delay generator
                DelayGenerator = args =>
                {
                    var delay = args.AttemptNumber switch
                    {
                        0 => TimeSpan.Zero,
                        1 => TimeSpan.FromSeconds(5),
                        _ => TimeSpan.FromSeconds(10)
                    };

                    return new ValueTask<TimeSpan?>(delay);
                },
                OnRetry = static args =>
                {
                    Console.WriteLine("Retrying, Attempt: {0}", args.AttemptNumber + 1);

                    // Event handlers can be asynchronous; here, we return an empty ValueTask.
                    return default;
                }
            });
            return resiliencePipeline;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles 5xx and 408 responses
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound) // Handles 503 responses
                .WaitAndRetryAsync(3, retryAttempt => {
                    Console.WriteLine("Retrying, Attempt: {0}", retryAttempt);
                    var delay = retryAttempt switch
                    {
                        0 => TimeSpan.Zero,
                        1 => TimeSpan.FromSeconds(5),
                        _ => TimeSpan.FromSeconds(10)
                    };

                    return delay;
                });
        }
        #endregion
    }
}
