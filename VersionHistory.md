# Version History

## Pending

Describe changes here when they're committed to the `master` branch. Move them to **Released** when the project version number is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]`, or `[patch]` in accordance with [Semantic Versioning](https://semver.org/).

## Released

### 0.7.0 Beta 1

* Add `netstandard2.1` target framework.
* Update Faithlife.Utility dependency to 0.9.0-beta1.

### 0.6.0

* Only target `netstandard2.0`.
* Build with C# 8.
* Add nullability annotations to public API.

### 0.5.0

* Remove dependency on Faithlife.Analyzers.

### 0.4.0

* Update minimum target frameworks to .NET Standard 2.0, .NET 4.7.2.

### 0.3.1

* [patch] Reuse `DefaultContractResolver` in `JsonUtility` to benefit from its caching. Fixes performance regression in 0.3.0.

### 0.3.0

* [major] Remove `ReadOnlyDictionaryJsonConverter`  and `DictionaryKeysAreNotPropertyNamesJsonConverter`. (Json.NET 9+ doesn't need them.)
* [minor] Add `OptionalJsonConverter` and use by default with `JsonUtility`.
* [minor] Add `DefaultValueDefaultAttribute` (used on `Optional<T>` properties to distinguish null from missing).
* [major] Remove empty `IsoDateTimeOffsetJsonConverter` constructor.

### 0.2.0

* [major] Change .NET Framework minimum version to 4.6.1.
* [major] Remove `DefaultValueDefaultAttribute`.
* [major] Remove `JsonPointer` and `JsonPatch`.
* [major] Simplify `JsonInputSettings` and `JsonOutputSettings`.
* [major] Rename `JsonFilter.AlternatePathSepartor` to `AlternatePathSeparator`.
* [major] Move JToken-specific members from `JsonUtility` to `JTokenUtility`.

### 0.1.1

* No changes in this release.

### 0.1.0

* Initial release.
