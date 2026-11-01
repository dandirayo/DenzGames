using System.Diagnostics;
using System.IO;
using DenzGames.Launcher.Models;
using Microsoft.Win32;

namespace DenzGames.Launcher.Services;

public sealed class PrerequisiteService
{
    public Task<PrerequisiteStatus> CheckAsync(string id, PrerequisiteDefinition definition)
    {
        return Task.Run(() =>
        {
            try
            {
                var installed = definition.Check.Type.ToLowerInvariant() switch
                {
                    "registry" => CheckRegistry(definition.Check),
                    "file" => CheckFile(definition.Check),
                    "command" => CheckCommand(definition.Check),
                    _ => false
                };

                return new PrerequisiteStatus(
                    id,
                    definition.Name,
                    installed,
                    installed ? "Terpasang dan siap digunakan." : definition.Description);
            }
            catch (Exception ex)
            {
                return new PrerequisiteStatus(id, definition.Name, false, $"Pemeriksaan gagal: {ex.Message}");
            }
        });
    }

    private static bool CheckRegistry(PrerequisiteCheck check)
    {
        if (string.IsNullOrWhiteSpace(check.Key) || string.IsNullOrWhiteSpace(check.Value))
        {
            return false;
        }

        const string prefix = "HKLM\\";
        if (!check.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var subKeyPath = check.Key[prefix.Length..];
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var subKey = baseKey.OpenSubKey(subKeyPath);
        var actual = subKey?.GetValue(check.Value);
        return actual is not null && string.Equals(
            Convert.ToString(actual), check.Expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool CheckFile(PrerequisiteCheck check)
    {
        return !string.IsNullOrWhiteSpace(check.Path) &&
               File.Exists(Environment.ExpandEnvironmentVariables(check.Path));
    }

    private static bool CheckCommand(PrerequisiteCheck check)
    {
        if (string.IsNullOrWhiteSpace(check.Command))
        {
            return false;
        }

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = check.Command,
            Arguments = check.Arguments ?? string.Empty,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });

        if (process is null)
        {
            return false;
        }

        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit(10_000);
        return process.ExitCode == 0 &&
               (string.IsNullOrEmpty(check.Contains) || output.Contains(check.Contains, StringComparison.OrdinalIgnoreCase));
    }
}

