# JTokenUtility.AsInt32 method

Returns an Int32 corresponding to the JToken if possible.

```csharp
public static int? AsInt32(this JToken? jToken)
```

## Return Value

This method returns null if the JToken is null, or if it doesn't contain a number, or if that number overflows an Int32, or if that number was parsed as floating-point.

## See Also

* class [JTokenUtility](../JTokenUtility.md)
* namespace [Faithlife.Json](../../Faithlife.Json.md)

<!-- DO NOT EDIT: generated by xmldocmd for Faithlife.Json.dll -->
