using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Revo.Examples.Todos.Tests.Integration.Support
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Asserts the response succeeded, including the response body (e.g. a server exception) in the failure.
        /// </summary>
        public static async Task EnsureSuccessAsync(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new XunitException(
                    $"Request failed with {(int) response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
