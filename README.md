[![](https://img.shields.io/nuget/v/Soenneker.Extensions.Object.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Object/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.object/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.object/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.Object.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.Object/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.object/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.object/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.Object
Object guards, numeric type checks, and JSON HTTP content using caller-supplied serialization metadata.

## Installation

```bash
dotnet add package Soenneker.Extensions.Object
```

## Create request bodies

```csharp
using Soenneker.Extensions.Object;

using HttpContent content = request.ToHttpContent(MyJsonContext.Default.SearchRequest);
```

`ToHttpContent()`, `ToHttpContentAndString()`, and `ToHttpContentWithKey()` accept `JsonTypeInfo<T>` metadata. Their `Try` variants return null results and optionally log conversion errors. The caller owns and must dispose returned content objects.

## Build query strings without reflection

```csharp
string query = request.ToQueryString(MyJsonContext.Default.SearchRequest);
```

Pass source-generated `JsonTypeInfo<T>` metadata, as with the HTTP-content helpers. Names, ignore conditions, and converters follow that metadata. Names and values are percent-encoded, booleans and numbers use JSON formatting, and nested objects and arrays are encoded as JSON. Null inputs and empty objects return an empty string; non-object JSON roots are rejected.

## Guards

- `IsObjectNumeric()` checks whether an object's runtime type is numeric; null throws.
- `ThrowIfNull()` throws `ArgumentNullException` and uses the calling member name unless a name is supplied.

## Reflection extensions

For `ToDictionaryViaReflection()`, query strings, form encoding, property diagnostics, and `ToHttpContentViaReflection()`, install [Soenneker.Extensions.Objects.Reflection](https://github.com/soenneker/soenneker.extensions.objects.reflection) and import `Soenneker.Extensions.Objects.Reflection`.
