using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.IntegrationTests.TestApp.Reads.Model
{
    [TablePrefix(NamespacePrefix = "ITEST", ColumnPrefix = "TLI")]
    public class TestListReadModel : EntityReadModel
    {
        public string Name { get; set; }
    }
}
