using System.Text;
using MyPlatform.Shared.Utils.Helpers;

namespace MyPlatform.SDK.ServiceCommunication.Http;

/// <summary>
/// Interface for service HTTP client.
/// </summary>
public interface IServiceHttpClient
{
    /// <summary>
    /// Sends a GET request.
    /// </summary>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="headers">Optional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response.</returns>
    Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="request">The request body.</param>
    /// <param name="headers">Optional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response.</returns>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="request">The request body.</param>
    /// <param name="headers">Optional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response.</returns>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="headers">Optional headers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of service HTTP client.
/// </summary>
public class ServiceHttpClient : IServiceHttpClient
{
    private readonly HttpClient _httpClient;

    public ServiceHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonHelper.Deserialize<TResponse>(content);
    }

    /// <inheritdoc />
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonHelper.Serialize(request), Encoding.UTF8, "application/json")
        };
        AddHeaders(httpRequest, headers);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonHelper.Deserialize<TResponse>(content);
    }

    /// <inheritdoc />
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonHelper.Serialize(request), Encoding.UTF8, "application/json")
        };
        AddHeaders(httpRequest, headers);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonHelper.Deserialize<TResponse>(content);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers is null) return;

        foreach (var header in headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}
