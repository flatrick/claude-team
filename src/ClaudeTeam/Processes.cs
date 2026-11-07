using System.ComponentModel;
using System.Diagnostics;

namespace ClaudeTeam;

public static class Processes
{
    public const int NotFound = 127;

    public static (int ExitCode, string Output) Capture(string file, params string[] arguments)
    {
        var info = new ProcessStartInfo(file) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        try
        {
            using var process = Process.Start(info)!;
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, output);
        }
        catch (Win32Exception)
        {
            return (NotFound, "");
        }
    }

    public static int Run(Command command, IReadOnlyDictionary<string, string> environment)
    {
        var info = new ProcessStartInfo(command.File) { UseShellExecute = false };
        foreach (var (key, value) in environment) info.Environment[key] = value;
        foreach (var argument in command.Arguments) info.ArgumentList.Add(argument);
        try
        {
            using var process = Process.Start(info)!;
            process.WaitForExit();
            return process.ExitCode;
        }
        catch (Win32Exception ex)
        {
            Console.Error.WriteLine($"Cannot start '{command.File}'. Install it and put it on PATH. {ex.Message}");
            return NotFound;
        }
    }
}
