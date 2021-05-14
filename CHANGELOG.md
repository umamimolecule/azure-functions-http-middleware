# Changelog

<a name="2.1.0" />

## [2.1.0] - 2021-05-14

### Added
 - Custom response handlers for body and query parameter validation failures

## [2.0.0] - 2020-12-12

### Changed
 - Updated references to Azure Functions SDK version 3.0.11 and .Net Core 3.1

<a name="1.3.0" />

## [1.3.0] - 2019-12-28

### Added
 - Add pipeline branching and conditional middleware via `MapWhen` and `UseWhen`.

<a name="1.2.2" />

## [1.2.2] - 2019-12-26

### Added
 - Example code for exception handler middleware.
 - Include symbols package when publishing NuGet package.

<a name="1.2.1" />

## [1.2.1] - 2019-12-02

### Added
 - More tests for middleware and other classes.

<a name="1.2.0" />

## [1.2.0] - 2019-12-01

### Added
 - XML documentation for all public classes and members so that consumers get Intellisense.
 - Add licence file.
 - Add StyleCop analyzer to enforce standards.

### Changed
 - Fixed bug where body stream could not be read after validation middleware was run.
 - Update example code in readme.

<a name="1.1.0" />

## [1.1.0] - 2019-11-30

### Changed
 - Use `HttpContext` from ASP .Net Core instead of custom HttpFunctionContext object.

<a name="1.0.0" />

## [1.0.0] - 2019-11-24
 - Initial release.