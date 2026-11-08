using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Odyssey.Core.Services;

internal static class AppUpdateService
{
    private const string RepositoryUrl = "https://github.com/zendev1710/Odyssey";

    public static async Task CheckAndDownloadAsync()
    {
        UpdateManager updateManager = CreateUpdateManager();
        if (!updateManager.IsInstalled)
        {
            return;
        }

        UpdateInfo? update = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);
        if (update is null)
        {
            return;
        }

        Trace.TraceInformation($"[UPDATER] Downloading version {update.TargetFullRelease.Version}.");
        await updateManager.DownloadUpdatesAsync(update).ConfigureAwait(false);
        Trace.TraceInformation("[UPDATER] Update downloaded; it will be applied the next time Odyssey starts.");
    }

    private static UpdateManager CreateUpdateManager()
    {
#if DEBUG
        string? localUpdateSource = Environment.GetEnvironmentVariable("ODYSSEY_UPDATE_SOURCE");
        if (!string.IsNullOrWhiteSpace(localUpdateSource))
        {
            return new UpdateManager(localUpdateSource);
        }
#endif

        return new UpdateManager(new GithubSource(RepositoryUrl, accessToken: null, prerelease: false));
    }
}