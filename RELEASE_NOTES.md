# RELEASE NOTES

## [1.34.1] - 2023-04-04

### Fixed
- set Hangfire JobStorage early to prevent errors when calling it before its server starts

## [1.34.0] - 2023-03-17

### Changed
- removed NLog dependency, logging replaced with standard Microsoft.Extensions.Logging; if you want to keep using NLog, you have to install it in your projects and set-up the adapters on your own

## [1.33.0] - 2023-01-15

### Added
- added configuration options for Hangfire processing server

### Changed
- Hangfire dashboard is now disabled by default
- updated NuGet metadata to use new preferred format (package icon, license expression, packaged README), added EmbedUntrackedSources
- breaking: all core packages now target .NET 6.0/7.0

## [1.32.0] - skipped

## [1.31.0] - 2022-12-26

### Added
- added build targets for .NET 7 with ASP.NET Core 7

### Changed
- upgraded optional AutoMapper extension to v12.0
- upgraded to Entity Framework Core 7.0

## [1.30.3] - 2022-11-10

### Fixed
- in EF Core, when synchronously projecting multiple times in a single tx (e.g. because projector itself publishes events), do not project the already projected events again

## [1.30.2] - 2022-10-12

### Fixed
- CRUD aggregate store now correctly deletes aggregates when the action also triggers the removal of other related entities

## [1.30.1] - 2022-06-16

### Added
- added StoreDate metadata to events published by CRUD aggregates

### Fixed
- CRUD aggregate store now throws exception / returns null for Get*/Find* methods when the aggregate is pending deletion

## [1.30.0] - 2022-05-17

### Fixed
- fixed IDatabaseInitializer sorting with InitializeAfterAttribute (was only comparing neighbors) & added support for transitive dependencies

### Changed
- Now targeting .NET 6.0 and .NET Standard 2.0 (dropping support for .NET Framework, .NET 3.1 and .NET Standard 2.0)
- Upgraded to ASP.NET Core 6.0, Entity Framework Core 6.0, AutoMapper 11

## [1.29.0] - 2022-03-01

### Added
- EasyNetQ custom service registration

## [1.28.1] - 2022-02-03

### Fixed
- fixed BufferedNotificationStore creating the same buffer multiple times
- fixed InMemoryCrudRepository returning non-saved entries & added tests

## [1.28.0] - 2022-01-27

### Changed
- breaking change (notifications extension): notification buffers, pipeline and governors are now identified using arbitrary string names instead of GUIDs for more flexibility

## [1.27.2] - 2021-12-15

### Fixed

- returning null values in JsonMetadata

## [1.27.1] - 2021-12-12

### Changed

- Fixed NuGet release

## [1.27.0] - 2021-12-12

### Added
- CrudEntityEventToPocoProjector now sets read model TenantId from metadata of first event if no TenantAggregateRootCreated is published (e.g. for non-event sourced entities)

### Fixed

- ProjectionEventListener now runs the async projections with correct tenant context from the events

### Changed

- MetadataExtensions.GetPublishDate and GetStoreDate now return a nullable value

## [1.26.1] - 2021-10-21

### Fixed

- EasyNetQ subscription AddType configuration should provide default empty configuration callback when passed null

## [1.26.0] - 2021-10-20

### Added
- EasyNetQ subscriptions can now be registered as blocking (i.e. processed sequentially, waiting until the event listeners complete before processing next message)

### Changed
- major version updates for dependencies: AutoMapper 10, EasyNetQ 6

## [1.25.1] - 2021-07-27

### Fixed
- fixed LambdaCommandBusExtensions.SendLambdaCommandAsync overloads that did not return value, added 'Async' suffix to method name for consistency

## [1.25.0] - 2021-07-27

### Added
- added SendLambdaCommand extension: easily executes specified lambda like it was performed by any regular command handler

## [1.24.0] - 2021-06-08

### Changed
- by default, event source and event queue catch-ups now don't block application startup until finished (IAsyncEventPipelineConfiguration.WaitForEventCatchUpsUponStartup)

## [1.23.0] - 2021-05-19

### Added
- database migration hooks - can now listen to events like DatabaseMigrationBeforeAppliedEvent, DatabaseMigrationAppliedEvent and DatabaseMigrationsCommittedEvent
- transactionMode for database migrations (isolated, without) for overriding their transaction behavior

### Fixed
- ExternalEventSourceCatchUp should dispatch messages with metadata mapped from the event record (e.g. ID)
- async events might occasionally have gotten enqueued twice during EF Core's coordinated transaction
- support for generic commands (still without automatic discovery of generic command handlers, though, needs to register manually)

### Changed
- database migrations are run inside a unit of work when using EF Core and EF6 providers
- EF Core/EF6 migration providers no longer create database upon startup
- renamed lifecycle hook interfaces (IApplicationStartedListener, IApplicationStoppingListener) and added new IApplicationStartingListener hook

### Improved
- better performance when enqueueing async events & for EFCoreCrudRepository.IsAttached

## [1.22.0] - 2021-04-30

### Added
- added support for event serializer customization

### Fixed
- external events and events published by non-event sourced aggregates are not stored to DB if not dispatched to any async event queues (no need to store them then)
- UseAllEFCoreInfrastructure/UseAllEF6Infrastructure did not run advancedAction, if supplied

### Changed
- breaking change: BasicEventMetadataNames moved to Revo.Core.Events
- manually published events no longer require explicitly set event message ID

## [1.21.0] - 2021-02-16

### Added
- added option to rerun repeatable database migrations when their dependencies get updated (RerunRepeatableMigrationsOnDependencyUpdate - default to true)

## [1.20.2] - 2021-02-12

### Fixed
- JobRunner was causing AmbiguousMatchException when a job handler class implemented more IJobHandler interfaces

## [1.20.1] - 2020-12-19

### Fixed
- missing IQueryTranslationPlugin in EF Core's internal DI container

## [1.20.0] - 2020-12-19

### Added
- custom query provider + helpers in EF Core provider for easier query authorization (default enabled by EnableCustomQueryProvider) with AuthorizationQueryableExtensions
- support for custom EF Core query translation plugins (IQueryTranslationPlugin)
- IEFCoreReadRepository.FromSqlInterpolated<T> and FromSqlRaw<T> (which apply repository filters, unlike existing IEFCoreDatabaseAccess methods)
- IEntityQueryFilter<TBase>.FilterAsync<T> overload without command (just takes the current one)

### Changed
- IEntityQueryFilter<TBase>.FilterAsync<T> filter is now generic to enable easier filtering for derived entity types

## [1.19.0] - 2020-12-06

### Change
- breaking change: TenantRepositoryFilter gets injected always, but uses now only Lazy<ITenantContext> - fixes issue when first request didn't have the filter applied (hint: use ExcludeFilters method when implementing a custom ITenantProvider)

## [1.18.2] - 2020-11-20

### Fixed
- Permission constructor now correctly accepts objects as contextId and resourceId  (in accordance with change from 1.18.0)

## [1.18.1] - 2020-11-16

### Fixed
- fixed publishing of NuGet package symbols: now using separate snupkg for symbols

## [1.18.0] - 2020-11-16

### Added
- support for .NET 5.0, ASP.NET Core 5.0 and EF Core 5.0 (supporting both 3.1 and 5.0 versions now)

### Changed
- Permission.ContextId and ResourceId can now be any objects
- updated minor version for EF Core 3.1.x
- updated RavenDB.Client to 5.0.4, Revo.RavenDB now only requires netstandard2.0
- Rebus:
    - using manual configuration instead of XML now
    - updated dependency to 5.0.4
    - Revo.Rebus now only requires netstandard2.0

### Removed
- dropping support for legacy ASP.NET platform (.NET 4.7)

## [1.17.0] - 2020-08-25

### Changed
- removed core dependency on AutoMapper
  - the functionality of automatic registration of profiles got separated into new Revo.Extensions.AutoMapper package
  - updated to AutoMapper 9.0

### Fixed
- Apply methods inherited in EntityEventProjector no longer get called multiple times
- catching ReflectionTypeLoadException exceptions in TypeExplorer

## [1.16.0] - 2020-06-23

### Added
- improved multi-tenancy configuration
- major refactoring of command bus internals:
  - support for multiple command buses (local, remote...)
  - routing commands to buses with command gateway
  - overhauled command pipeline based on new and more flexible command bus middlewares
  - CommandExecutionOptions that can change the tenant context to run the command in and other options
- AsyncEventListenerBindingExtensions for easier async event listener registration
- convenience methods IReadRepository.GetManyAsync<T> and IReadRepository.FindManyAsync<T> using Guid IDs
- added PerAggregateAsyncEventSequencer and PerTenantAsyncEventSequencer for common sequencer scenarios
- added IDatabaseInitializerLoader.EnsureDatabaseInitialized

### Changed
- breaking change: null tenants now cannot access other tenant's data by default (can be changed by configuration)
- breaking change: ITenant moved from Revo.Domain to Revo.Core
- simplified security (IUserManager replaced with Revo.Core.Security.IClaimsPrincipalResolver, disabling null implementations can now be done with CoreConfigurationSection.Security.UseNullSecurityModule)
- Permission now contains only PermissionTypeId instead of PermissionType
- Repository can now be instantiated even without any active UnitOfWork
- updated to (ASP).NET Core 3.1
- ASP.NET Core provider's RevoStartup class now only calls AddMvcCore() instead of AddMvc(), you have to add other parts yourself in ConfigureServices(...) if you want them

### Removed
- Revo.AspNetCore.Security.ISignInManager removed, use IUserContext instead
- AuthorizePermissionsAttribute for old ASP.NET 4 removed (inconsistent with the rest of the framework)

## [1.15.0] - 2020-03-05

### Added
- IDatabaseMigrationExecutionOptions.MigrateOnlySpecifiedModules can now be specified using wildcards

### Fixed
- database migrations: doesn't throw anymore if there is no migration for specified version, but DB is already up-to-date
- database migrations: now throws when updating to 'latest' version, but no migrations are found

## [1.14.1] - 2020-02-26

### Added
- IReadRepository.GetManyAsync and FindManyAsync

### Fixed
- fixed appending events to event store (wrong expected event number)

## [1.14.0] - 2020-02-11

### Added
- event upgrade support - just implement IEventUpgrade in your code (auto discovery) and Revo upgrades the event streams on-the-fly upon loading the aggregates

### Fixed
- event version parsed from types whose name ends with V letter without number no longer recognized as versioned name

### Changed
- EventSourcedAggregateRoot.Commit increases Version by 1 (previously by event count)
- EntityEventToPocoProjector uses AggregateVersion instead of StreamSequenceNumber event metadata for read model versioning

### Removed
- removed APNS and FCM (push notification) channel support from Revo.Extensions.Notifications using obsolete PushSharp library

## [1.13.1] - 2020-01-15

### Added
- now it's possible to specify FileSqlDatabaseMigration version in its headers

## [1.13.0] - 2019-11-29

### Added
- database migrations - new own system for managing database schema version migrations (incl. module dependencies)
- standalone database migration tool Revo.Tools.DatabaseMigrator (also invokable as global .NET Core tool revo-dbmigrate)
- support for SQLite in EF Core infrastructure implementation (event store, async event queues, etc.)

### Fixed
- events are now correctly marked as dispatched even when there are no listeners for them
- fixed missing IExternalEventStore registration for EF6 provider
- not resetting DbContexts after EFCoreCoordinatedTransaction finishes

### Changed
- Entity Framework 6 provider updated to EF6.3 version and is now targeting both net472 and netstandard2.1
- Revo.Extensions.Notifications and Revo.Extensions.History now need to be explicitly registered in Revo configuration using AddHistoryExtension and AddNotificationsExtension
- removed IRepository.SaveChangesAsync to avoid confusion - it doesn't work well in certain situations (e.g. with EF Core provider) and IUnitOfWork.CommitAsync should be used instead
- NLog updated to 4.6.7

## [1.12.0] - 2019-10-22

### Added
- new BaseTypeAttribute for EF Core mapping - overrides the mapped entity base type

### Changed
- upgraded to ASP.NET Core 3.0 and Entity Framework Core 3.0
- ReadModelForEntityAttribute no longer overrides mapped table name (leaves default)

### Removed
- dropped OData out-of-the-box integration on ASP.NET & ASP.NET Core platforms altogether (OData is not supported on ASP.NET Core 3.0 now), you have to integrate it by yourself now

## [1.11.0] - 2019-09-03

### Added
- IRepository.FindManyAsync and GetManyAsync with optimized batch-loading of event sourced aggregates

### Changed
- IRepository.FindAllAsync now returns an array instead of IList<T>
- improved StaticClassifierDatabaseInitializer, now also supporting event sourced aggregates
- Cake script now uses VS2019 build toolset

### Fixed
- EF Core's PrefixConvention now correctly prefixes columns defined in abstract classes annotated with TablePrefixAttribute

## [1.10.0] - 2019-06-03

### Added
- IAsyncQueryableResolver.AnyAsync
- injection support for SignalR hubs in Revo.AspNetCore

## [1.9.1] - 2019-05-15

### Added
- IEventNumberVersioned and EventEntityReadModel for read models with additional arbitrary versioning (concurrency control)

### Changed
- now possible to configure ODataQuerySettings for EFCoreQueryableToODataResultConverter
- removed IEventSourcedAggregateRoot constraints from EF Core and EF6 projectors (now can be any IAggregateRoot)

### Fixed
- CrudAggregateStore now automatically removes entities that have been marked with IsDeleted
- FakeRepository now correctly removes entites that have been flagged as IsDeleted

## [1.9.0] - 2019-03-15

### Added
- new Revo.EasyNetQ module for simple integrations with RabbitMQ

## [1.8.0] - 2019-03-04

### Changed
- default EventSerializer and NotificationSerializer now use the same JSON serializer settings

### Fixed
- EF Core's PrefixConvention with deeper inheritance hierarchies
- EF Core sync projectors are invoked not invoked multiple times with the same events

### Removed
- obsolete Globalization stuff (messages, locales, translatable entities, ASP.NET localization helpers...)

## [1.7.0] - 2019-02-14

### Added

- new default in-memory job scheduler

### Changed

- FakeClock now uses AsyncLocal instead of ThreadLocal
- refactored Revo.Extensions.Notifications for .NET Core compatibility and better configuration

## [1.6.0] - 2019-01-17

### Changed

- projects upgraded to .NET Core 2.2, ASP.NET Core 2.2 and Entity Framework Core 2.2

## [1.5.0] - 2019-01-10

### Added
- automatic event projector discovery and registration (now enabled by default)
- event publishing for non-event sourced entities (not transactionally safe and not saving to event store yet, though, so beware)
- coordinated UoW transaction for EF Core including synchronous projections, publishing events from projections and fast event queue dispatching
- EFCoreInMemoryCrudRepository and EFCoreInMemoryDatabaseAccess
- recurring job scheduling support

### Fixed
- optimized event queue dispatching for big amounts of queues
- ODataAsyncResultFilter.DefaultConverter now returns correct counts
- fixed BasicDomainModelConvention when using inheritance including abstract types
- fixed binding of Hangfire and infrastructure configuration sections, added more options

## [1.4.0] - 2018-11-05

### Added
- [#1](https://github.com/revoframework/Revo/issues/1) **ASP.NET Core support** - platform implementation, i.e. user context, security, DI, OData, etc.
- [#2](https://github.com/revoframework/Revo/issues/2), [#3](https://github.com/revoframework/Revo/issues/3) **EF Core support** - data access & infrastructure (async events, sagas, projections...)
- Throttling async event processing to minimize the amount of running tasks and open DB connections (when running event queue catch-ups during app start-up and pseudo-synchronously processing events after a request; see IAsyncEventPipelineConfiguration)

### Changed
- **flattened and simplified package structure** (now provider-centric) - vendor-specific modules were moved
to Providers directory and some of them were merged/renamed:
  - _Revo.Platforms.AspNet → Revo.AspNet_
  - _Revo.DataAccess.EF6 and Revo.Infrastructure.EF6 → Revo.EF6_
  - _Revo.DataAccess.RavenDB → Revo.RavenDB_
  - _Revo.Integrations.Rebus → Revo.Rebus_
- **improved AsyncEventWorker concurrency** - framework did not prevent multiple workers from parallel processing of one async event queue, causing occasional (e.g. when under heavy load) concurrency exceptions
(which are eventually handled by an automatic retry); now allowing only one active worker per a queue in an application instance
- **Ninject binding extensions InRequestOrJobScope** is now two separate methods with corresponding fallbacks in order - InTaskScope (mostly preferred) and InRequestScope

### Removed
- **IAutoMapperDefinition** - removed as obsolete and replaced with AutoMapper's own profiles (auto-discovered again)
- **removed implicit ASP.NET Web API configuration** - i.e. default OData and serializer settings
- **TokenValidator** - removed as obsolete
- **(Tenant)ContextSequence** - removed (possibly to be reimplemented later)

### Fixed
- **CRUD repositories** now correctly wrap their **concurrency exceptions as OptimisticConcurrencyException**
- **objects from DI container** are now deterministically disposed at the end of tasks run within a context (also reducing amount of unused open DB connection)
- **Hangfire job task context disposal** now correctly awaits the command to finish

## [1.3.0] - 2018-08-31
### Fixed
- **EF6SagaMetadataRepository** bug not including the keys in a query
- **EF6 saga keys** not being reloaded correctly due to JSON serialization changing dictionary key character cases
- **ExecuteCommandJob** is now resolved correctly by Hangfire

### Added
- **(I)UserPermissionAuthorizer** for manual and more fine-grained user action authorization
- **EF6/EntityEventProjector** for more arbitrary projections

### Changed
- framework configuration - framework and its module parameters now need to be programmatically configured and set-up using the newly introduced **RevoConfiguration**
- **PermissionAuthorizer** renamed to PermissionAuthorizationMatcher
- **Default ASP.NET Web API JSON ContractResolver** switched to DefaultContractResolver with CamelCaseNamingStrategy from CamelCasePropertyNamesContractResolver
(matches default ASP.NET Core behavior and does not change dictionary key character cases)
- Hangfire integration for background job processing got its own **Revo.Infrastructure.Hangfire** package

## [1.2.0] - 2018-07-16
- First public version released.