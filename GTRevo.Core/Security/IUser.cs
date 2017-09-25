using System;

namespace GTRevo.Core.Security
{
    public interface IUser
    {
        Guid Id { get; }
        string UserName { get; }
    }
}
