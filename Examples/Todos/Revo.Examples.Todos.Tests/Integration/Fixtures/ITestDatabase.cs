using System;

namespace Revo.Examples.Todos.Tests.Integration.Fixtures
{
    /// <summary>
    /// Empty database created for a test, for the Todos app to run against.
    /// </summary>
    public interface ITestDatabase : IAsyncDisposable
    {
        /// <summary>
        /// The Todos app's DatabaseProvider setting (also the name of its connection string).
        /// </summary>
        string Provider { get; }

        /// <summary>
        /// Name of the EF Core database provider the app is expected to use with this database.
        /// </summary>
        string EFCoreProviderName { get; }

        string ConnectionString { get; }
    }
}
