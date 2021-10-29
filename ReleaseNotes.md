# Release Notes

## 0.7.1

* Improve nullability annotation of `JTokenUtility.IsNull()`.

## 0.7.0

* Add `netstandard2.1` target framework.
* Update Faithlife.Utility dependency to 0.9.0-beta1.

## 0.6.0

* Only target `netstandard2.0`.
* Build with C# 8.
* Add nullability annotations to public API.

## 0.5.0

* Remove dependency on Faithlife.Analyzers.

## 0.4.0

* Update minimum target frameworks to .NET Standard 2.0, .NET 4.7.2.

## 0.3.1

* Reuse `DefaultContractResolver` in `JsonUtility` to benefit from its caching. Fixes performance regression in 0.3.0.

## 0.3.0

* **Breaking:** Remove `ReadOnlyDictionaryJsonConverter`  and `DictionaryKeysAreNotPropertyNamesJsonConverter`. (Json.NET 9+ doesn't need them.)
* Add `OptionalJsonConverter` and use by default with `JsonUtility`.
* Add `DefaultValueDefaultAttribute` (used on `Optional<T>` properties to distinguish null from missing).
* **Breaking:** Remove empty `IsoDateTimeOffsetJsonConverter` constructor.

## 0.2.0

* **Breaking:** Change .NET Framework minimum version to 4.6.1.
* **Breaking:** Remove `DefaultValueDefaultAttribute`.
* **Breaking:** Remove `JsonPointer` and `JsonPatch`.
* **Breaking:** Simplify `JsonInputSettings` and `JsonOutputSettings`.
* **Breaking:** Rename `JsonFilter.AlternatePathSepartor` to `AlternatePathSeparator`.
* **Breaking:** Move JToken-specific members from `JsonUtility` to `JTokenUtility`.

## 0.1.1

* No changes in this release.

## 0.1.0

* Initial release.
