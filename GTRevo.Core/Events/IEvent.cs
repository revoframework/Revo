using System;
using MediatR;

namespace GTRevo.Core.Events
{
    public interface IEvent : INotification
    {
        Guid Id { get; }
    }
}
