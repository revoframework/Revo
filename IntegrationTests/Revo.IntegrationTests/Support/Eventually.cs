using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Revo.IntegrationTests.Support
{
    public static class Eventually
    {
        /// <summary>
        /// Polls the condition until it holds, failing if it does not within the timeout.
        /// </summary>
        public static async Task HoldsAsync(Func<Task<bool>> condition, string because,
            TimeSpan? timeout = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var maxDuration = timeout ?? TimeSpan.FromSeconds(15);

            while (!await condition())
            {
                if (stopwatch.Elapsed > maxDuration)
                {
                    throw new TimeoutException($"Condition not met within {maxDuration.TotalSeconds}s: {because}");
                }

                await Task.Delay(50);
            }
        }
    }
}
