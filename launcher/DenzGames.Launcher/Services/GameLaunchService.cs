using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using DenzGames.Launcher.Models;

namespace DenzGames.Launcher.Services;

public sealed class GameLaunchService(string repositoryRoot)
{
    private readonly string _repositoryRoot = Path.GetFullPath(repositoryRoot);
    private readonly string _installRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DenzGames", "games");

    public bool CanPrepare(GameDefinition game, out string message)
    {
        var directExecutable = ResolveSourcePath(game, game.Executable);
        if (directExecutable is not null && File.Exists(directExecutable))
        {
            message = "Build game ditemukan dan siap dimainkan.";
            return true;
        }

        var installedExecutable = ResolveInstalledExecutable(game);
        if (installedExecutable is not null && File.Exists(installedExecutable))
        {
            message = "Game sudah disiapkan dan siap dimainkan.";
            return true;
        }

        var archive = ResolveSourcePath(game, game.Archive);
        if (archive is not null && File.Exists(archive) && !string.IsNullOrWhiteSpace(game.ArchiveExecutable))
        {
            message = "Build tersedia dalam arsip dan akan diekstrak saat tombol Play ditekan.";
            return true;
        }

        if (Uri.TryCreate(game.DownloadUrl, UriKind.Absolute, out var downloadUri) &&
            downloadUri.Scheme == Uri.UriSchemeHttps && !string.IsNullOrWhiteSpace(game.Sha256))
        {
            message = "Build akan diunduh dan diverifikasi saat tombol Play ditekan.";
            return true;
        }

        message = "Build game tidak ditemukan. Periksa executable, archive, atau downloadUrl di manifest.";
        return false;
    }

    public async Task<string> PrepareAndLaunchAsync(GameDefinition game, CancellationToken cancellationToken = default)
    {
        var executable = ResolveSourcePath(game, game.Executable);
        if (executable is null || !File.Exists(executable))
        {
            executable = await PrepareInstalledBuildAsync(game, cancellationToken);
        }

        if (!File.Exists(executable))
        {
            throw new FileNotFoundException("Executable game tidak ditemukan setelah proses persiapan.", executable);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = Path.GetDirectoryName(executable)!,
            UseShellExecute = true
        });

        return executable;
    }

    private async Task<string> PrepareInstalledBuildAsync(GameDefinition game, CancellationToken cancellationToken)
    {
        var installedExecutable = ResolveInstalledExecutable(game)
                                  ?? throw new InvalidDataException("archiveExecutable belum diatur di manifest.");
        if (File.Exists(installedExecutable))
        {
            return installedExecutable;
        }

        var archive = ResolveSourcePath(game, game.Archive);
        string? downloadedArchive = null;
        if (archive is null || !File.Exists(archive))
        {
            downloadedArchive = await DownloadArchiveAsync(game, cancellationToken);
            archive = downloadedArchive;
        }

        var destination = GetInstallDirectory(game);
        Directory.CreateDirectory(destination);
        try
        {
            ZipFile.ExtractToDirectory(archive, destination, overwriteFiles: true);
        }
        catch
        {
            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, recursive: true);
            }

            throw;
        }
        finally
        {
            if (downloadedArchive is not null && File.Exists(downloadedArchive))
            {
                File.Delete(downloadedArchive);
            }
        }

        return installedExecutable;
    }

    private static async Task<string> DownloadArchiveAsync(GameDefinition game, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(game.DownloadUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidDataException("downloadUrl HTTPS belum tersedia untuk game ini.");
        }

        if (string.IsNullOrWhiteSpace(game.Sha256) || game.Sha256.Length != 64)
        {
            throw new InvalidDataException("SHA256 wajib diisi sebelum build dapat diunduh.");
        }

        var temporaryFile = Path.Combine(Path.GetTempPath(), $"denzgames-{game.Id}-{Guid.NewGuid():N}.zip");
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(20) };
        await using (var source = await client.GetStreamAsync(uri, cancellationToken))
        await using (var destination = File.Create(temporaryFile))
        {
            await source.CopyToAsync(destination, cancellationToken);
        }

        await using var file = File.OpenRead(temporaryFile);
        var actualHash = Convert.ToHexString(await SHA256.HashDataAsync(file, cancellationToken));
        if (!actualHash.Equals(game.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(temporaryFile);
            throw new InvalidDataException("SHA256 hasil unduhan tidak cocok. File ditolak.");
        }

        return temporaryFile;
    }

    private string? ResolveSourcePath(GameDefinition game, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var sourceRoot = Path.GetFullPath(Path.Combine(_repositoryRoot, game.SourcePath));
        EnsureChildPath(_repositoryRoot, sourceRoot);
        var result = Path.GetFullPath(Path.Combine(sourceRoot, relativePath));
        EnsureChildPath(sourceRoot, result);
        return result;
    }

    private string? ResolveInstalledExecutable(GameDefinition game)
    {
        if (string.IsNullOrWhiteSpace(game.ArchiveExecutable))
        {
            return null;
        }

        var installDirectory = GetInstallDirectory(game);
        var result = Path.GetFullPath(Path.Combine(installDirectory, game.ArchiveExecutable));
        EnsureChildPath(installDirectory, result);
        return result;
    }

    private string GetInstallDirectory(GameDefinition game) =>
        Path.Combine(_installRoot, game.Id, game.Version);

    private static void EnsureChildPath(string parent, string child)
    {
        var normalizedParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedChild = Path.GetFullPath(child);
        if (!normalizedChild.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Path manifest keluar dari direktori yang diizinkan.");
        }
    }
}
