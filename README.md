[![](https://img.shields.io/nuget/v/Soenneker.Extensions.Object.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Object/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.object/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.object/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.Object.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Object/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.object/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.object/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.Object
A collection of helpful Object extension methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.Object
```

## Quick start

```csharp
using Soenneker.Extensions.Object;

// Given an existing object named obj:
var result = obj.IsObjectNumeric();
```

## Common operations

- `IsObjectNumeric()` - Determines whether the specified object is of a numeric type.
- `ThrowIfNull()` - Throws an `ArgumentNullException` if the input object is null.
- `ToDictionary()` - Converts an object's public instance properties into a dictionary. Uses `JsonPropertyNameAttribute` if present for the key. Only declared properties (not inherited) are included. Optimized for low allocations and high performance. Returns a dictionary of property names (or JSON names) to values.
- `ToQueryStringViaReflection()` - Uses Reflection to build a query string out of an object. If object is null, returns an empty string. Uses the object's property names as the keys of the query string.
- `ToQueryString()` - Builds a query string from cached property accessors. Uses the object's property name OR 'JsonPropertyName' attribute as the keys of the query string. Escapes the value. Uses JSON-compatible property names and scalar formatting without serializing the containing object.
- `LogNullProperties()` - Logs any properties of the given object that are null.
- `LogNullPropertiesRecursivelyAsJson()` - Logs the properties of an object that are null, including nested objects, as a JSON string.
- `ToReadableString()` - Converts an object into a human-readable string representation, including its public properties. Returns a formatted string representation of the object's properties and their values.
- `ToFormUrlEncodedContent()` - Converts an object into FormUrlEncodedContent using reflection, honoring JsonPropertyName attributes on properties. Only top-level, readable instance properties are included.
- `ToHttpContent()` - Converts an object to an `HttpContent` with JSON content using `JsonUtil.WebOptions`. Returns an `HttpContent` containing the JSON representation of the object. If the object is `null`, returns an `HttpContent` with empty content. This method does not log the result.
- `ToHttpContentWithKey()` - Serializes an object to JSON, constructs an `HttpContent`, and adds an 'x-api-key' header to the request. Returns an `HttpContent` instance containing the serialized JSON content with the specified 'x-api-key' header added.
- `TryToHttpContent()` - Attempts to convert an object to an `HttpContent` with JSON content, logging any serialization errors. Returns an `HttpContent` containing the JSON representation of the object, or `null` if serialization fails.

The package also includes one additional operation for more specialized cases.

## Usage
