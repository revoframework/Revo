
<p align="center">
<img src="https://github.com/revoframework/Revo/blob/develop/res/revo-framework.png" alt="Revo framework">
</p>
<p align="center">
<a href="https://dev.azure.com/revoframework/Revo/_build?definitionId=1&view=runs"><img src="https://img.shields.io/azure-devops/build/revoframework/7ff8258b-dd3c-4007-9d06-7609742e93cf/1/develop?style=flat-square&logo=azure-pipelines" alt="Build status"></a>
<a href="https://codecov.io/gh/revoframework/Revo"><img src="https://img.shields.io/codecov/c/github/revoframework/Revo?logo=codecov&style=flat-square" alt="Code coverage"></a>
<a href="https://github.com/revoframework/Revo/releases"><img src="https://img.shields.io/github/release-date/revoframework/Revo?label=latest%20release&style=flat-square" alt="Latest release date"></a>
<a href="https://www.nuget.org/packages?q=revo"><img src="https://img.shields.io/nuget/v/Revo.Core?logo=NuGet&style=flat-square" alt="NuGet packages"></a>
<a href="https://pkgs.dev.azure.com/revoframework/_packaging/revoframework/nuget/v3/index.json"><img src="https://img.shields.io/badge/nuget%20CI-source-blue?logo=NuGet&style=flat-square" alt="CI NuGet feed at Azure"></a><br>
<a href="https://revoframework.gitbook.io/revo/"><img src="https://img.shields.io/badge/docs-GITBOOK-blue.svg?style=flat-square" alt="Docs"></a>
<a href="https://gitter.im/revoframework/Revo?utm_source=share-link&utm_medium=link&utm_campaign=share-link"><img src="https://img.shields.io/gitter/room/revoframework/Revo?color=4BB595&logo=gitter&&style=flat-square" alt="Gitter chat"></a>
<a href="https://github.com/revoframework/Revo/pulls"><img src="https://img.shields.io/badge/PRs-welcome-green?style=flat-square" alt="PRs welcome"></a>
<a href="https://github.com/revoframework/Revo/blob/develop/LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square" alt="License"></a>
</p>

# Revo Framework

Revo is an application framework for modern server C\#/.NET applications built with _event sourcing_, _CQRS_ and _DDD_.

_Development of this framework is supported by <a href="https://olify.io">OLIFY - smarter solution for facility management & maintenance.</a>_

## Contents

* 🏠 [Home](#revo-framework)
* ⚡️ [Features](#features)
* 👓 [Super-short example](#show-me-how-it-looks)
* 🚀 [Getting started](#getting-started)
* 📘 [Full documentation](https://docs.revoframework.net/)
* 📦 [Packages](#packages)
* 📑 [License](#license)

## ⚡️ Features

The project combines the concepts of event sourcing, CQRS and DDD to provide framework for building applications that are scalable, maintainable, can work in distributed environments and are easy to integrate with outside world. As such, it takes some rather opinionated approaches on the design of certain parts of its architecture. Revo also offers other common features and infrastructure that is often necessary for building complete applications – for example, authorizations, validations, messaging, integrations, multi-tenancy or testing.
Furthermore, its extensions implement other useful features like entity history change-tracking, auditing or user notifications.

[**Domain-Driven Design**](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks)  
Building blocks for rich DDD-style domain models \([aggregates](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#aggregates), [entities](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#entities), [value objects](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#value-objects), [domain events](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#domain-events), [repositories](https://revoframework.gitbook.io/revo/reference-guide/data-persistence#aggregate-repository)...\).

[**Event Sourcing**](https://revoframework.gitbook.io/revo/reference-guide/events)  
Implementing event-sourced entity persistence with support for multiple event store backends \(PostgreSQL, MSSQL, SQLite...\).

[**CQRS**](https://revoframework.gitbook.io/revo/reference-guide/commands-and-queries)  
Segregating command and query responsibilities with:
* [Commands and queries](https://revoframework.gitbook.io/revo/reference-guide/commands-and-queries#commands-queries)
* Command/query [handlers](https://revoframework.gitbook.io/revo/reference-guide/commands-and-queries#command-query-handlers)
* Processing pipeline with filters for cross-cutting concerns \([authorization](https://revoframework.gitbook.io/revo/reference-guide/authorization), [validation](https://revoframework.gitbook.io/revo/reference-guide/validation)\)
* [Different read/write models](https://revoframework.gitbook.io/revo/reference-guide/projections)

[**A/synchronous event processing**](https://revoframework.gitbook.io/revo/reference-guide/events)  
Support for both [synchronous](https://revoframework.gitbook.io/revo/reference-guide/events#synchronous-event-processing) and [asynchronous](https://revoframework.gitbook.io/revo/reference-guide/events#asynchronous-event-processing) event processing, guaranteed _at-least-once_ delivery, event queues with strict sequence ordering \(optionally\), event source catch-ups, optional [pseudo-synchronous event dispatch](https://revoframework.gitbook.io/revo/reference-guide/events#pseudo-synchronous-event-dispatch) for listeners \(projectors, for example\).

[**Data access**](https://revoframework.gitbook.io/revo/reference-guide/data-persistence)  
Thin abstraction layer for easy data persistence (e.g. querying read models) using _Entity Framework Core_, _Entity Framework 6_, _RavenDB,_ testable _in-memory database_ or other data providers. Includes support for simple [database migrations](https://revoframework.gitbook.io/revo/reference-guide/database-migrations).

[**Projections**](https://revoframework.gitbook.io/revo/reference-guide/projections)  
Support for read-model projections with various backends \(e.g. _Entity Framework Core_ (_PostgreSQL_, _MSSQL_, _SQLite_,...), _Entity Framework 6_, _RavenDB_...\), automatic idempotency- and concurrency-handling, etc.

[**SOA, messaging and integration**](https://revoframework.gitbook.io/revo/reference-guide/integrations)  
Scale and integrate by [publishing and receiving events](https://docs.revoframework.net/~/drafts/-LaA9ji7E8zsIXVOg-qo/primary/reference-guide/integrations#rabbitmq-messaging-with-easynetq), commands and queries using common messaging patterns,<br>
e.g. with _RabbitMQ_ message queue (using _EasyNetQ_ connector or _Rebus_ service bus).

[**Sagas**](https://revoframework.gitbook.io/revo/reference-guide/sagas)  
Coordinating long-running processes or inter-aggregate cooperation with sagas that react to events<br>\(a.k.a. _process managers_\).

[**Authorization**](https://revoframework.gitbook.io/revo/reference-guide/authorization)  
Basic permission/role-based ACL for commands and queries, fine-grained row filtering.

**Other minor features:**
* [**Validation**](https://revoframework.gitbook.io/revo/reference-guide/validation) for commands, queries and other structures.
* [**Jobs**](https://revoframework.gitbook.io/revo/reference-guide/jobs)
* [**Multi-tenancy**](https://revoframework.gitbook.io/revo/reference-guide/multi-tenancy)  
* [**Event message metadata**](https://revoframework.gitbook.io/revo/reference-guide/events#event-messages-and-metadata)
* [**Event versioning**](https://revoframework.gitbook.io/revo/reference-guide/events#event-versioning)
* [**Event upgrades**](https://revoframework.gitbook.io/revo/reference-guide/events#event-upgrades)
* [**Database migrations**](https://revoframework.gitbook.io/revo/reference-guide/database-migrations)
* **History and change-tracking**
* **User notifications:** event-based, with different output channels (mail, etc.), aggregation, buffering, etc.
* **.NET Core 3.0+, .NET Standard 2.0+ support (with integration for ASP.NET Core)**

## 👓 Show me how it looks!

Super-short example of a simple application that can save tasks using event-sourced aggregates and then query them back from a RDBMS.

### Event

The event that happens when changing a task's name.

```C#
public class TodoRenamedEvent : DomainAggregateEvent
{
    public TodoRenamedEvent(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
```

### Aggregate

The task aggregate root.

```C#
public class Todo : EventSourcedAggregateRoot
{
    public Todo(Guid id, string name) : base(id)
    {
        Rename(name);
    }
    
    protected Todo(Guid id) : base(id)
    {
    }
    
    public string Name { get; private set; }
    
    public void Rename(string name)
    {
        if (!Name != name)
        {
            Publish(new TodoRenamedEvent(name));
        }
    }
    
    private void Apply(TodoRenamedEvent ev)
    {
        Name = ev.Name;
    }
}
```

### Command and command handler

Command to save a new task.

```C#
public class CreateTodoCommand : ICommand
{
    public CreateTodoCommand(string name)
    {
        Name = name;
    }

    [Required]
    public string Name { get; }
}
```

```C#
public class TodoCommandHandler : ICommandHandler<CreateTodoCommand>
{
    private readonly IRepository repository;
    
    public TodoCommandHandler(IRepository repository)
    {
        this.repository = repository;
    }
    
    public Task HandleAsync(CreateTodoCommand command, CancellationToken cancellationToken)
    {
        var todo = new Todo(command.Id);
        todo.Rename(command.Name);
        repository.Add(todoList);
        return Task.CompletedTask;
    }   
}
```

### Read model and projection

Read model and a projection for the event-sourced aggregate.

```C#
public class TodoReadModel : EntityReadModel
{
    public string Name { get; set; }
}
```

```C#
public class TodoListReadModelProjector : EFCoreEntityEventToPocoProjector<Todo, TodoReadModel>
{
    public TodoListReadModelProjector(IEFCoreCrudRepository repository) : base(repository)
    {
    }
    
    private void Apply(IEventMessage<TodoRenamedEvent> ev)
    {
        Target.Name = ev.Event.Name;
    }
}
```

### Query and query handler

Query to read the tasks back from a RDBMS.

```C#
public class GetTodosQuery : IQuery<IQueryable<TodoReadModel>>
{
}
```

```C#
public class TaskQueryHandler : IQueryHandler<GetTodoQuery, IQueryable<TodoReadModel>>
{
    private readonly IReadRepository readRepository;
    
    public TaskListQueryHandler(IReadRepository readRepository)
    {
        this.readRepository = readRepository;
    }
    
    public Task<IQueryable<TodoReadModel>> HandleAsync(GetTodoListsQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(readRepository
            .FindAll<TodoListReadModel>());
    }
}
```

## 🚀 Getting started

If you are new to the framework, you can
 * begin with reading the quick walkthrough for the [**Simple TO-DOs example**](https://docs.revoframework.net/general/example-simple-to-dos-task-list-app) (a task list app)
 * or try exploring [~~**the other examples**~~](https://github.com/revoframework/Revo/tree/develop/Examples) (TODO!) and framework sources on Github.
 
You can also start by reading the 📘 [**reference guide**](https://docs.revoframework.net/general/getting-started).

## 📦 Packages

Released version are available in form of NuGet packages.  
There is also a separate [**pre-release CI package feed at Azure**](https://pkgs.dev.azure.com/revoframework/_packaging/revoframework/nuget/v3/index.json).

<div style="width:100%";>
<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Core
    </th>
  </tr>
  <tr>
    <td>
      Revo.Core
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Core/"><img src="https://img.shields.io/nuget/v/Revo.Core.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.DataAccess
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.DataAccess/"><img src="https://img.shields.io/nuget/v/Revo.DataAccess.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Domain
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Domain/"><img src="https://img.shields.io/nuget/v/Revo.Domain.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Infrastructure
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Infrastructure/"><img src="https://img.shields.io/nuget/v/Revo.Infrastructure.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Testing
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Testing/"><img src="https://img.shields.io/nuget/v/Revo.Testing.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>

<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Data access
    </th>
  </tr>
  <tr>
    <td>
      Revo.EFCore
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.EFCore/"><img src="https://img.shields.io/nuget/v/Revo.EFCore.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.EF6
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.EF6/"><img src="https://img.shields.io/nuget/v/Revo.EF6.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.RavenDB
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.RavenDB/"><img src="https://img.shields.io/nuget/v/Revo.RavenDB.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>


<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Platforms
    </th>
  </tr>
  <tr>
    <td>
      Revo.AspNetCore
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.AspNetCore/"><img src="https://img.shields.io/nuget/v/Revo.AspNetCore.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Hangfire
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Hangfire/"><img src="https://img.shields.io/nuget/v/Revo.Hangfire.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>

<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Integrations
    </th>
  </tr>
  <tr>
    <td>
      Revo.EasyNetQ (RabbitMQ connector)
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.EasyNetQ/"><img src="https://img.shields.io/nuget/v/Revo.EasyNetQ.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Rebus (service bus, deprecated)
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Rebus/"><img src="https://img.shields.io/nuget/v/Revo.Rebus.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>

<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Other extensions
    </th>
  </tr>
  <tr>
    <td>
      Revo.Extensions.History
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Extensions.History/"><img src="https://img.shields.io/nuget/v/Revo.Extensions.History.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Extensions.Notifications
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Extensions.Notifications/"><img src="https://img.shields.io/nuget/v/Revo.Extensions.Notifications.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>
</div>

Most applications will require at least **Revo.Core**, **Revo.DataAccess**, **Revo.Domain**, **Revo.Infrastructure** packages to get started with and then typically a platform package like **Revo.Platforms.AspNetCore** (ASP.NET Core platform implementation) and a data-access package like **Revo.EFCore** (for Entity Framework Core support).

## 📑 License

> MIT License
> 
> Copyright (c) 2017-2020 Martin Zima<br>
> Copyright (c) 2017-2020 Olify IO s.r.o.
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH > THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
