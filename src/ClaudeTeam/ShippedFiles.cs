using System.Security.Cryptography;
using System.Text;

namespace ClaudeTeam;

// A shipped file: its path under .claude/ and its embedded text.
public sealed record ShippedFile(string PublishedPath, string Text);

public static class ShippedFiles
{
    // Install order: the rule, the agent, then the skills.
    public static IReadOnlyList<ShippedFile> All { get; } =
    [
        new($"rules/{PeerRule.FileName}", PeerRule.Text),
        new($"agents/{LeaderAgent.FileName}", LeaderAgent.Text),
        .. TeamSkills.Names.Select(name => new ShippedFile($"skills/{name}/{TeamSkills.FileName}", TeamSkills.Text(name))),
    ];
}

public static class ShippedHashes
{
    public const string ResourceName = "shipped-hashes.txt";

    // Lines are "<published path> <sha256 hex>". The list holds every shipped version of each file.
    private static readonly HashSet<(string Path, string Hash)> Entries = Installer.ReadEmbedded(ResourceName)
        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(line => line.Split(' ', 2))
        .Where(parts => parts.Length == 2)
        .Select(parts => (parts[0], parts[1].Trim().ToLowerInvariant()))
        .ToHashSet();

    public static string Sha256(string text)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text.ReplaceLineEndings("\n"))));

    public static bool Contains(string publishedPath, string text) => Entries.Contains((publishedPath, Sha256(text)));
}
