namespace Revo.Tools.DatabaseMigrator;

/// <summary>
/// Implemented by types in assemblies passed to <c>--assemblies</c>. Use <see cref="DatabaseMigratorBindings"/>
/// to register embedded SQL migrations.
/// </summary>
public interface IDatabaseMigratorPlugin
{
    void Configure(DatabaseMigratorBindings bindings);
}
