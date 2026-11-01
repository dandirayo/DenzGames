using System.Text.Json.Serialization;
using System.Windows;

namespace DenzGames.Launcher.Models;

public sealed class GameDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Version { get; init; }
    public required string Engine { get; init; }
    public required string SourcePath { get; init; }
    public string? Executable { get; init; }
    public string? Archive { get; init; }
    public string? ArchiveExecutable { get; init; }
    public string? DownloadUrl { get; init; }
    public string? Sha256 { get; init; }
    public IReadOnlyList<string> Prerequisites { get; init; } = [];

    [JsonIgnore]
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name[..1].ToUpperInvariant();
}

public sealed class PrerequisiteDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required PrerequisiteCheck Check { get; init; }
    public required string Url { get; init; }
}

public sealed class PrerequisiteCheck
{
    public required string Type { get; init; }
    public string? Key { get; init; }
    public string? Value { get; init; }
    public string? Expected { get; init; }
    public string? Path { get; init; }
    public string? Command { get; init; }
    public string? Arguments { get; init; }
    public string? Contains { get; init; }
}

public sealed record GameCatalog(
    string RepositoryRoot,
    IReadOnlyList<GameDefinition> Games,
    IReadOnlyDictionary<string, PrerequisiteDefinition> Prerequisites);

public sealed record PrerequisiteStatus(string Id, string Name, bool IsInstalled, string Message)
{
    public string Symbol => IsInstalled ? "✓" : "!";
    public Visibility HelpVisibility => IsInstalled ? Visibility.Collapsed : Visibility.Visible;

    public static PrerequisiteStatus MissingDefinition(string id) =>
        new(id, id, false, "Definisi prasyarat tidak ditemukan di manifest.");
}

