using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;

namespace Revo.Examples.Minimal.Messages.Commands;

public class CreateTodoListCommand : ICommand
{
    public required Guid Id { get; init; }

    [Required]
    public required string Name { get; init; }
}