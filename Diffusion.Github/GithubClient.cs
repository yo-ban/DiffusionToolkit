using System.Net;
using System.Text.Json;

namespace Diffusion.Github;

public class GithubClient : IDisposable
{
    private static readonly HttpClientHandler _sharedHandler = new HttpClientHandler
    {
        AllowAutoRedirect = false
    };

    private static readonly HttpClient _sharedClient = new HttpClient(_sharedHandler, disposeHandler: false);

    private readonly HttpClient _client;
    private readonly string _user;
    private readonly string _repo;
    private const string _userAgent = "GithubClient/1.0";

    public GithubClient(string user, string repo)
    {
        _client = _sharedClient;

        _user = user;
        _repo = repo;
    }

    public Task<IEnumerable<Release>?> GetReleases()
    {
        return GetReleases(CancellationToken.None);
    }

    public Task<IEnumerable<Tag>?> GetTags()
    {
        return GetTags(CancellationToken.None);
    }

    public Task<Stream> DownloadAsync(string url)
    {
        return DownloadAsync(url, CancellationToken.None);
    }

    public async Task<IEnumerable<Release>?> GetReleases(CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.github.com/repos/{_user}/{_repo}/releases"));
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("User-Agent", _userAgent);
        using var response = await _client.SendAsync(request, token);
        var json = await response.Content.ReadAsStringAsync(token);
        return JsonSerializer.Deserialize<IEnumerable<Release>>(json);
    }

    public async Task<IEnumerable<Tag>?> GetTags(CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.github.com/repos/{_user}/{_repo}/tags"));
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("User-Agent", _userAgent);
        using var response = await _client.SendAsync(request, token);
        var json = await response.Content.ReadAsStringAsync(token);
        return JsonSerializer.Deserialize<IEnumerable<Tag>>(json);
    }

    public Task<Stream> DownloadAsync(string url, CancellationToken token)
    {
        return DownloadAsync(url, token, maxRedirects: 10);
    }

    private async Task<Stream> DownloadAsync(string url, CancellationToken token, int maxRedirects)
    {
        if (maxRedirects < 0)
        {
            throw new InvalidOperationException("Too many redirects while downloading.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, UriKind.Absolute));
        request.Headers.Add("Accept", "application/octet-stream");
        request.Headers.Add("User-Agent", _userAgent);

        var response = await _client.SendAsync(request, token);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Moved:
            case HttpStatusCode.Found:
            {
                // The response stream is not used for redirects; dispose before recursing.
                string? redirect = null;
                if (response.Headers.TryGetValues("Location", out var locations))
                {
                    redirect = locations.FirstOrDefault();
                }
                response.Dispose();

                if (string.IsNullOrEmpty(redirect))
                {
                    throw new InvalidOperationException("Redirect response is missing a valid Location header.");
                }

                return await DownloadAsync(redirect, token, maxRedirects - 1);
            }
            case HttpStatusCode.OK:
                // The returned stream is backed by this response; the caller owns it and
                // disposal of the stream returns the underlying connection to the pool.
                return await response.Content.ReadAsStreamAsync(token);
            default:
                response.Dispose();
                throw new Exception($"Unexpected status code when downloading: {response.StatusCode}");
        }
    }

    public void Dispose()
    {
    }
}