using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LmsMobile.Models;

namespace LmsMobile.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly SessionService _session;

    public AuthHeaderHandler(SessionService session) => _session = session;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _session.Token;
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}

public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly SessionService _session;

    public ApiClient(HttpClient http, SessionService session)
    {
        _http = http;
        _session = session;
    }

    public Task<T> GetAsync<T>(string url) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url));

    public Task<T> PostAsync<T>(string url, object body) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: Json)
        });

    public Task<T> PutAsync<T>(string url, object body) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(body, options: Json)
        });

    public Task DeleteAsync(string url) =>
        SendAsync<object>(new HttpRequestMessage(HttpMethod.Delete, url));

    private async Task<T> SendAsync<T>(HttpRequestMessage request)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, "Cannot reach the server. Check your connection and that the API is running.");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(0, "The server took too long to respond. Please try again.");
        }
        finally
        {
            request.Dispose();
        }

        using (response)
        {
            ApiResponse<T>? body = null;
            try
            {
                body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(Json);
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                // Non-JSON body: fall back to a generic message below.
            }

            // An expired or rejected token sends the user back to Sign In.
            if (response.StatusCode == HttpStatusCode.Unauthorized && _session.TryEnd())
            {
                await MainThread.InvokeOnMainThreadAsync(AppShell.SessionExpiredAsync);
                throw new ApiException(401, "Your session has expired. Please sign in again.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = body?.Errors is { Count: > 0 }
                    ? string.Join("\n", body.Errors.SelectMany(e => e.Value))
                    : !string.IsNullOrWhiteSpace(body?.Message)
                        ? body!.Message
                        : $"Request failed ({(int)response.StatusCode}).";
                throw new ApiException((int)response.StatusCode, message);
            }

            if (body is null)
                throw new ApiException((int)response.StatusCode, "The server returned an unexpected response.");

            return body.Data!;
        }
    }
}
