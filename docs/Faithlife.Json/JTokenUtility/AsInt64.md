# JTokenUtility.AsInt64 method

Returns an Int64 corresponding to the JToken if possible.

```csharp
public static long? AsInt64(this JToken? jToken)
```

## Return Value

This method returns null if the JToken is null, or if it doesn't contain a number, or if that number overflows an Int64, or if that number was parsed as floating-point.

## See Also

* class [JTokenUtility](../JTokenUtility.md)
* namespace [Faithlife.Json](../../Faithlife.Json.md)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.Json.dll -->
