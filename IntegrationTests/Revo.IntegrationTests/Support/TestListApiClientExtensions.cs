using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Revo.IntegrationTests.Support
{
    public static class TestListApiClientExtensions
    {
        public static async Task<TestListDto[]> GetListsAsync(this HttpClient client)
        {
            return await client.GetFromJsonAsync<TestListDto[]>("/lists");
        }

        public static async Task<TestListDto> FindListAsync(this HttpClient client, Guid id)
        {
            return (await client.GetListsAsync()).SingleOrDefault(x => x.Id == id);
        }
    }
}
