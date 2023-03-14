using System;
using FluentAssertions;
using Revo.Examples.Todos.Domain;
using Revo.Examples.Todos.Domain.Events;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.Examples.Todos.Tests.Domain
{
    public class TodoListTests
    {
        private TodoList sut;

        public TodoListTests()
        {
            sut = new TodoList(Guid.Parse("E16EF2AB-20FF-438E-A1CE-48E6A8BDC7B2"), "My todo list");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Constructor(bool loadEventsOnly)
        {
            sut.AssertConstructorEvents(x =>
                {
                    x.Id.Should().Be(Guid.Parse("E16EF2AB-20FF-438E-A1CE-48E6A8BDC7B2"));
                    x.Name.Should().Be("My todo list");

                }, loadEventsOnly,
                new TodoListRenamedEvent("My todo list"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Rename(bool loadEventsOnly)
        {
            sut.AssertEvents(
                x =>
                {
                    x.Rename("Groceries");
                },
                x =>
                {
                    x.Name.Should().Be("Groceries");
                },
                loadEventsOnly,
                new TodoListRenamedEvent("Groceries"));
        }
    }
}
