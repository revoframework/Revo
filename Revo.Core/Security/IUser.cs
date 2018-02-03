using System;

namespace Revo.Core.Security
{
    public interface IUser
    {
        Guid Id { get; }
        string UserName { get; }
    }
}
