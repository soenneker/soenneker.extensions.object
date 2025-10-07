using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Soenneker.Constants.Auth;
using Soenneker.Utils.Json;

namespace Soenneker.Extensions.Object;

public static partial class ObjectExtension
{
    private static readonly byte[] _emptyByteArray = [];

    /// <summary>
    /// Converts an object to an <see cref="HttpContent"/> with JSON content using <see cref="JsonUtil.WebOptions"/>.
    /// </summary>
    /// <param name="obj">The object to serialize into JSON content.</param>
    /// <returns>
    /// An <see cref="HttpContent"/> containing the JSON representation of the object.
    /// If the object is <c>null</c>, returns an <see cref="HttpContent"/> with empty content.
    /// </returns>
    /// <remarks>
    /// This method does not log the result. For logging options, see <see cref="JsonUtil"/>.
    /// </remarks>
    [Pure]
    public static HttpContent ToHttpContent(this object? obj)
    {
        HttpContent httpContent;

        if (obj is null)
        {
            httpContent = new ByteArrayContent(_emptyByteArray)
            {
                Headers = { ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json) }
            };
        }
        else
        {
            byte[] utf8Bytes = JsonUtil.SerializeToUtf8Bytes(obj);

            httpContent = new ByteArrayContent(utf8Bytes)
            {
                Headers = { ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json) }
            };
        }

        return httpContent;
    }

    /// <summary>
    /// Serializes an object to JSON and constructs an <see cref="HttpContent"/> with the JSON content.
    /// </summary>
    /// <param name="obj">The object to serialize into JSON.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><description>An <see cref="HttpContent"/> instance with the serialized JSON content.</description></item>
    ///   <item><description>The serialized JSON string used to create the <see cref="HttpContent"/>.</description></item>
    /// </list>
    /// If the object is <c>null</c>, the method returns:
    /// <list type="bullet">
    ///   <item><description>An <see cref="HttpContent"/> with empty content.</description></item>
    ///   <item><description>An empty string as the serialized JSON.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Uses JsonUtil.WebOptions for JSON serialization.
    /// </remarks>
    [Pure]
    public static (HttpContent httpContent, string str) ToHttpContentAndString(this object? obj)
    {
        string? jsonContent = obj != null ? JsonUtil.Serialize(obj) : "";

        var content = new StringContent(jsonContent!, Encoding.UTF8, MediaTypeNames.Application.Json);
        return (content, jsonContent!);
    }

    /// <summary>
    /// Serializes an object to JSON, constructs an <see cref="HttpContent"/>, and adds an 'x-api-key' header to the request.
    /// </summary>
    /// <param name="obj">The object to serialize into JSON.</param>
    /// <param name="apiKey">The API key to include in the 'x-api-key' header.</param>
    /// <returns>
    /// An <see cref="HttpContent"/> instance containing the serialized JSON content 
    /// with the specified 'x-api-key' header added.
    /// </returns>
    /// <remarks>
    /// This method calls <see cref="ToHttpContent"/> to create the <see cref="HttpContent"/> 
    /// and adds the 'x-api-key' header using HttpHeaders.TryAddWithoutValidation.
    /// </remarks>
    [Pure]
    public static HttpContent ToHttpContentWithKey(this object? obj, string apiKey)
    {
        var httpContent = obj.ToHttpContent();
        httpContent.Headers.TryAddWithoutValidation(AuthConstants.XApiKey, apiKey);

        return httpContent;
    }
}