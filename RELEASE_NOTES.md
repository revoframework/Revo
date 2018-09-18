# RELEASE NOTES

## [1.4.0] - unreleased (_develop_)

### Added
- [#1](https://github.com/revoframework/Revo/issues/1) **ASP.NET Core support** - platform implementation, i.e. user context, security, DI, OData, etc.
- [#2](https://github.com/revoframework/Revo/issues/2), [#3](https://github.com/revoframework/Revo/issues/3) **EF Core support** - data access & infrastructure (async events, sagas, projections...)

### Changed
- **flattened and simplified package structure** (now provider-centric) - vendor-specific modules were moved
to Providers directory and some of them merged/renamed:  
  - _Revo.Platforms.AspNet → Revo.AspNet_
  - _Revo.DataAccess.EF6 and Revo.Infrastructure.EF6 → Revo.EF6_  
  - _Revo.DataAccess.RavenDB → Revo.RavenDB_  
  - _Revo.Integrations.Rebus → Revo.Rebus_

### Removed
- **IAutoMapperDefinition** - removed as obsolete and replaced with AutoMapper's own profiles (auto-discovered again)
- **removed implicit ASP.NET Web API configuration** - i.e. default OData and serializer settings

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