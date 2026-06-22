using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Github;

namespace Diffusion.Common;

public class UpdateChecker : IDisposable
{
    public GithubClient Client { get; }
    private CancellationTokenSource _cts;
    private bool _disposed;

    public void Cancel()
    {
        _cts.Cancel();
    }

    public CancellationToken CancellationToken => _cts.Token;

    public Release? LatestRelease { get; private set; }

    public UpdateChecker()
    {
        _cts = new CancellationTokenSource();
        Client = new GithubClient("RupertAvery", "DiffusionToolkit");
    }

    private async Task<Release?> GetLatestRelease()
    {
        var releases = await Client.GetReleases(_cts.Token);

        if (releases == null)
        {
            return null;
        }

        return releases.OrderByDescending(r => r.published_at).FirstOrDefault();
    }



    public async Task<bool> CheckForUpdate(string? path = null)
    {
        var latest = await GetLatestRelease();

        if (latest == null || latest.tag_name == null)
        {
            return false;
        }

        LatestRelease = latest;

        var localVersion = SemanticVersionHelper.GetLocalVersion(path);

        if (!SemanticVersion.TryParse(latest.tag_name, out var releaseVersion))
        {
            return false;
        }

        return releaseVersion > localVersion;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts?.Dispose();
                Client?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
