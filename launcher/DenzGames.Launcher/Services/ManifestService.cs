using System.IO;
using System.Text.Json;
using DenzGames.Launcher.Models;

namespace DenzGames.Launcher.Services;

public sealed class ManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GameCatalog> LoadAsync(CancellationToken cancellationToken = default)
    {
        var root = FindRepositoryRoot();
        var manifestDirectory = Path.Combine(root, "manifest");
        var gamesPath = Path.Combine(manifestDirectory, "games.json");
        var prerequisitesPath = Path.Combine(manifestDirectory, "prerequisites.json");

        await using var gamesStream = File.OpenRead(gamesPath);
        await using var prerequisitesStream = File.OpenRead(prerequisitesPath);
        var games = await JsonSerializer.DeserializeAsync<List<GameDefinition>>(gamesStream, JsonOptions, cancellationToken)
                    ?? throw new InvalidDataException("games.json kosong atau tidak valid.");
        var prerequisites = await JsonSerializer.DeserializeAsync<Dictionary<string, PrerequisiteDefinition>>(
                                prerequisitesStream, JsonOptions, cancellationToken)
                            ?? throw new InvalidDataException("prerequisites.json kosong atau tidak valid.");

        return new GameCatalog(root, games, prerequisites);
    }

    private static string FindRepositoryRoot()
    {
        string? packagedRoot = null;
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "manifest", "games.json")))
                {
                    if (Directory.Exists(Path.Combine(current.FullName, "games")))
                    {
                        return current.FullName;
                    }

                    packagedRoot ??= current.FullName;
                }

                current = current.Parent;
            }
        }

        if (packagedRoot is not null)
        {
            return packagedRoot;
        }

        throw new FileNotFoundException("Folder manifest DenzGames tidak ditemukan.");
    }
}

