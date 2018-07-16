
<p align="center">
<img src="https://github.com/revoframework/Revo/blob/develop/res/revo-framework.png" alt="Revo framework">
</p>
<p align="center">
<a href="https://ci.appveyor.com/project/revoframework/revo/"><img src="https://ci.appveyor.com/api/projects/status/uil4j7y3nlqkdmy1/branch/develop?svg=true" alt="Build status"></a>
<a href="https://www.nuget.org/packages?q=revo"><img src="https://img.shields.io/nuget/v/Revo.Core.svg" alt="NuGet package version"></a>
<a href="https://revoframework.gitbook.io/revo/"><img src="https://img.shields.io/badge/docs-GITBOOK-blue.svg" alt="Docs"></a>
<a href="https://github.com/revoframework/Revo/issues"><img src="https://img.shields.io/github/issues/revoframework/Revo.svg" alt="GitHub issues"></a>
<a href="https://github.com/revoframework/Revo/pulls"><img src="https://img.shields.io/badge/contributions-welcome-orange.svg" alt="Contributions"></a>
<a href="https://github.com/revoframework/Revo/blob/develop/LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License"></a>
</p>

# Revo Framework

Revo is an application framework for modern server C\#/.NET applications built with _event sourcing_, _CQRS_ and _DDD_.

## Table of contents

* [Home](#revo-framework)
* [Features](#features)
* [Getting started](#getting-started)
* [Examples](#examples)
* [Requirements](#requirements)
* [License](#license)

## Features

The framework combines the concepts of event sourcing, CQRS and DDD to provide support for building applications that are scalable, maintainable, can work in distributed environments and are easy to integrate with outside world. As such, it takes some rather opinionated approaches on the design of certain parts of its architecture. Revo also offers other common features and infrastructure that is often necessary for building complete applications – for example, authorizations, validations, messaging, integrations, multi-tenancy or testing.
Furthermore, its extensions implement other useful features like entity history change-tracking, auditing or user notifications.

[**Domain-Driven Design**](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks)  
Building blocks for rich DDD-style domain models \([aggregates](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#aggregates), [entities](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#entities), [value objects](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#value-objects), [domain events](https://revoframework.gitbook.io/revo/reference-guide/domain-building-blocks#domain-events), [repositories](https://revoframework.gitbook.io/revo/reference-guide/data-persistence#aggregate-repository)...\).

[**Event Sourcing**](https://revoframework.gitbook.io/revo/reference-guide/events)  
Implementing event-sourced entity persistence with multiple backends \(just _MSSQL_ for now\).

[**CQRS**](https://revoframework.gitbook.io/revo/reference-guide/commands-and-queries)  
Segregating command and query responsibilities with:
* [Commands and queries](https://revoframework.gitbook.io/revo/reference-guide/commands-and-queries#commands-queries)
* Command/query [handlers](https://revoframework.gitbook.io/revo/reference-guide/commands-and-queries#command-query-handlers)
* Processing pipeline with filters for cross-cutting concerns \([authorization](https://revoframework.gitbook.io/revo/reference-guide/authorization), [validation](https://revoframework.gitbook.io/revo/reference-guide/validation)\)
* [Different read/write models](https://revoframework.gitbook.io/revo/reference-guide/projections)

[**A/synchronous event processing**](https://revoframework.gitbook.io/revo/reference-guide/events)  
Support for both [synchronous](https://revoframework.gitbook.io/revo/reference-guide/events#synchronous-event-processing) and [asynchronous](https://revoframework.gitbook.io/revo/reference-guide/events#asynchronous-event-processing) event processing, guaranteed _at-least-once_ delivery, event queues with strict sequence ordering \(optionally\), event source catch-ups, optional [pseudo-synchronous event dispatch](https://revoframework.gitbook.io/revo/reference-guide/events#pseudo-synchronous-event-dispatch) for listeners \(projectors, for example\).

[**Data access**](https://revoframework.gitbook.io/revo/reference-guide/data-persistence)  
Abstraction layer for _Entity Framework 6_, _RavenDB,_ testable _in-memory database_ or other data providers.

[**Projections**](https://revoframework.gitbook.io/revo/reference-guide/projections)  
Support for read-model projections with various backends \(e.g. _MSSQL_/_Entity Framework 6_, _RavenDB_...\), automatic idempotency- and concurrency-handling, etc.

[**SOA and integration**](https://revoframework.gitbook.io/revo/reference-guide/integrations)  
Scale and integrate by publishing and receiving events, commands and queries using common messaging patterns,<br>e.g. with _RabbitMQ_ message queue and/or uses _Rebus_ service bus.

[**Sagas**](https://revoframework.gitbook.io/revo/reference-guide/sagas)  
Coordinating long-running processes or inter-aggregate cooperation with sagas that react to events<br>\(a.k.a. _process managers_\).

[**Authorization**](https://revoframework.gitbook.io/revo/reference-guide/authorization)  
Basic permission/role-based ACL for commands and queries, fine-grained row filtering.

**Other minor features:**
* [**Validation**](https://revoframework.gitbook.io/revo/reference-guide/validation) for commands, queries and other structures.
* [**Jobs**](https://revoframework.gitbook.io/revo/reference-guide/jobs)
* [**Multi-tenancy**](https://revoframework.gitbook.io/revo/reference-guide/multi-tenancy)  
* [	**Event message metadata**](https://revoframework.gitbook.io/revo/reference-guide/events#event-messages-and-metadata)
* [**Event versioning**](https://revoframework.gitbook.io/revo/reference-guide/events#event-versioning)
* **History and change-tracking**
* **User notifications:** event-based, with mail/APNS/FCM output channels, supporting aggregation, etc.
* **ASP.NET support** \(ASP.NET Core coming soon...\)

## Getting started

(TODO)

Binaries are up for grabs in form of NuGet packages:

<div style="width:100%";>
<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Core (.NET Standard 2.0)
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
      Entity Framework 6 (.NET Framework 4.7.1)
    </th>
  </tr>
  <tr>
    <td>
      Revo.DataAccess.EF6
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.DataAccess.EF6/"><img src="https://img.shields.io/nuget/v/Revo.DataAccess.EF6.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Infrastructure.EF6
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Infrastructure.EF6/"><img src="https://img.shields.io/nuget/v/Revo.Infrastructure.EF6.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>

<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      ASP.NET (.NET Framework 4.7.1)
    </th>
  </tr>
  <tr>
    <td>
      Revo.Platforms.AspNet
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Platforms.AspNet/"><img src="https://img.shields.io/nuget/v/Revo.Platforms.AspNet.svg" alt="NuGet package version"></a>
    </td>
  </tr>
  <tr>
    <td>
      Revo.Extensions.AspNet.Interop
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Extensions.AspNet.Interop/"><img src="https://img.shields.io/nuget/v/Revo.Extensions.AspNet.Interop.svg" alt="NuGet package version"></a>
    </td>
  </tr>
</table>

<table border="0" cellpadding="0" cellspacing="0" style="float:left;">
  <tr>
    <th colspan="2">
      Other integrations & extensions
    </th>
  </tr>
  <tr>
    <td>
      Revo.Integrations.Rebus
    </td>
    <td>
      <a href="https://www.nuget.org/packages/Revo.Integrations.Rebus/"><img src="https://img.shields.io/nuget/v/Revo.Integrations.Rebus.svg" alt="NuGet package version"></a>
    </td>
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

Most applications will require at least **Revo.Core**, **Revo.DataAccess**, **Revo.Domain**, **Revo.Infrastructure** packages to get started with and then typically **Revo.Platforms.AspNet** package (ASP.NET platform implementation) and **Revo.DataAccess.EF6** with **Revo.Infrastructure.EF6** package (for Entity Framework 6 support).

## Examples

(TODO)

For now, see the sample Hello World application in the examples folder ([Examples/Revo.Examples.HelloAspNet.Bootstrap](https://github.com/revoframework/Revo/tree/develop/Examples/Revo.Examples.HelloAspNet.Bootstrap)) and the [reference guide](https://revoframework.gitbook.io/revo/).

## Requirements

The framework is written in C\# 7.1 and targets the .NET Standard 2.0 specification; some of its modules currently also require the .NET Framework 4.7.1 where needed \(e.g. Entity Framework 6 support\). Revo also makes a heavy use of the C\# async/await pattern and uses the TAP \(Task Asynchronous Pattern\) throughout its entire codebase \(i.e. _async all the way_\).

## License

> MIT License
> 
> Copyright (c) 2017-2018 Martin Zima<br>
> Copyright (c) 2017-2018 ASP a.s.
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH > THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
