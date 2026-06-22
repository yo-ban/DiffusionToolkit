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

    public async Task<Stream> DownloadAsync(string url, CancellationToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, UriKind.Absolute));
        request.Headers.Add("Accept", "application/octet-stream");
        request.Headers.Add("User-Agent", _userAgent);

        var response = await _client.SendAsync(request, token);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Moved:
            {
                var redirect = response.Headers.GetValues("Location").First();
                response.Dispose();
                return await DownloadAsync(redirect, token);
            }
            case HttpStatusCode.Found:
            {
                var redirect = response.Headers.GetValues("Location").First();
                response.Dispose();
                return await DownloadAsync(redirect, token);
            }
            case HttpStatusCode.OK:
                return await response.Content.ReadAsStreamAsync(token);
            default:
                response.Dispose();
                throw new Exception("");
        }
    }

    public void Dispose()
    {
    }
}