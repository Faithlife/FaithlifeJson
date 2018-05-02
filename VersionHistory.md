# Version History

## Pending

Add changes here when they're committed to the `master` branch. Move them to "Released" once the version number
is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]` or `[patch]` in accordance with [SemVer](http://semver.org).

### 0.4.0

* [minor] Add `ValueTypeDto<T>` and `ValueTypeDtoJsonConverter` and use by default with `JsonUtility`.

## Released

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
