using System;
using MediatR;

namespace GTRevo.Events
{
    public interface IEvent : INotification
    {
        Guid Id { get; }
    }
}
