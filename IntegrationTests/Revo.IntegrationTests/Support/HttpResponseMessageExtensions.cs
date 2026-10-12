using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Revo.IntegrationTests.Support
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Asserts the response status code, including the response body (e.g. a server exception) in the failure.
        /// </summary>
        public static async Task ShouldHaveStatusAsync(this HttpResponseMessage response, HttpStatusCode expected)
        {
            if (response.StatusCode != expected)
            {
                string body = await response.Content.ReadAsStringAsync();
                throw new XunitException(
                    $"Expected status {(int) expected} {expected}, but got {(int) response.StatusCode} {response.StatusCode}:\n{body}");
            }
        }
    }
}
