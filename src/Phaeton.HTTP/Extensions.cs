using System.Text;
using System.Text.Json;
using System.Web;

namespace Phaeton.HTTP;

public static class Extensions
{
    public static async Task<T> Get<T>(
        this HttpClient httpClient,
        string uri,
        object? obj = null
    )
    {
        ThrowIfInvalidParams(
            httpClient,
            uri
        );

        AddQueryParams(
            ref uri,
            obj
        );

        var res = await httpClient.GetAsync($"{httpClient.BaseAddress}{uri}");
        var elo = await res.Content.ReadAs<dynamic>();

        if (!res.IsSuccessStatusCode)
            throw new Exception("External");

        return await res.Content.ReadAs<T>();
    }

    public static async Task Post(
        this HttpClient httpClient,
        string uri,
        object obj
    )
    {
        ThrowIfInvalidParams(
            httpClient,
            uri
        );

        var res = await httpClient.PostAsync(
            uri,
            new StringContent(
                Serialize(obj),
                Encoding.UTF8,
                "application/json"
            )
        );

        res.EnsureSuccessStatusCode();
    }

    public static async Task<T> Post<T>(
        this HttpClient httpClient,
        string uri,
        object obj
    )
    {
        ThrowIfInvalidParams(
            httpClient,
            uri
        );

        var res = await httpClient.PostAsync(
            uri,
            new StringContent(
                Serialize(obj),
                Encoding.UTF8,
                "application/json"
            )
        );

        res.EnsureSuccessStatusCode();

        return await res.Content.ReadAs<T>();
    }

    private static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string Serialize(object obj)
        => JsonSerializer.Serialize(
            obj,
            JsonSerializerOptions
        );

    private static T Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(
            json,
            JsonSerializerOptions
        );

    private static async Task<T> ReadAs<T>(this HttpContent content)
        => Deserialize<T>(await content.ReadAsStringAsync());

    private static void AddQueryParams(
        ref string uri,
        object? obj
    )
    {
        if (obj is null)
            throw new Exception("Object can not be null.");

        uri += '?' + string.Join(
            "&",
            Deserialize<IDictionary<string, string>>(Serialize(obj)).Select(x => HttpUtility.UrlEncode(x.Key) + "=" + HttpUtility.UrlEncode(x.Value))
        );
    }

    private static void ThrowIfInvalidParams(
        HttpClient http,
        string uri
    )
    {
        if (http is null)
            throw new ArgumentNullException(nameof(http));


        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException(
                "Can't be null or empty",
                nameof(uri)
            );
    }
}