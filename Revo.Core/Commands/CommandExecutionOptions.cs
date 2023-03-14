namespace Revo.Core.Commands
{
    /// <summary>
    /// Defines options for an execution of a command.
    /// </summary>
    /// <remarks>Note that some of the options may actually be used only if command is handled
    /// using a command handler with local command handler pipeline (e.g. they may have no
    /// significance if a remote command handler is registered for the command).</remarks>
    public class CommandExecutionOptions
    {
        public static readonly CommandExecutionOptions Default = new CommandExecutionOptions(null, null);

        public CommandExecutionOptions(bool? autoCommitUnitOfWork = null,
            CommandTenantContextOverride tenantContext = null)
        {
            AutoCommitUnitOfWork = autoCommitUnitOfWork;
            TenantContext = tenantContext;
        }

        /// <summary>
        /// Should the command handle middleware automatically start new unit of work and commit it upon
        /// its successful completion? Default to null, which implicitly enables this behavior for ICommand
        /// commands (not IQuery queries).
        /// </summary>
        public bool? AutoCommitUnitOfWork { get; }
        public CommandTenantContextOverride TenantContext { get; }

        public CommandExecutionOptions WithTenantContext(CommandTenantContextOverride tenantContextOverride)
        {
            return new CommandExecutionOptions(AutoCommitUnitOfWork, tenantContextOverride);
        }
    }
}