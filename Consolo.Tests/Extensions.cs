using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;
using System.IO;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using System.Text;

namespace Consolo.Tests;

public static class Extensions
{
    public static void ShouldMatchSnapshot(this string target, [CallerMemberName] string? snapshotName = null)
    {
        if (snapshotName is null)
        {
            throw new ArgumentNullException(nameof(snapshotName));
        }

        var snapshotPath = $"snapshots/{snapshotName}.cs.snapshot";

        if (Environment.GetEnvironmentVariable("WriteSnapshots") == "true")
        {
            File.WriteAllText(snapshotPath, target);
            Console.WriteLine($"Wrote {target.Length} chars to '{snapshotPath}'");

            Assert.Inconclusive();
        }

        if (!File.Exists(snapshotPath))
        {
            Assert.Fail($"Snapshot missing: `{snapshotPath}`");
        }

        var expected = File.ReadAllText(snapshotPath);
        var diff = InlineDiffBuilder.Diff(expected, target);

        if (diff.Lines.Any(l => l.Type != ChangeType.Unchanged))
        {
            var messageBuilder = new StringBuilder(
                $"Snapshot '{snapshotName}' does not match the current output. Run with `-e WriteSnapshots=true` to update.");

            foreach (var line in diff.Lines)
            {
                messageBuilder.Append(line.Position);

                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        messageBuilder.Append("+ ");

                        messageBuilder.AppendLine(line.Text);
                        break;
                    case ChangeType.Deleted:
                        messageBuilder.Append("- ");

                        messageBuilder.AppendLine(line.Text);
                        break;
                    case ChangeType.Unchanged:
                        break;
                    default:
                        messageBuilder.AppendLine(line.Text);
                        break;
                }
            }

            Assert.Fail(messageBuilder.ToString());
        }
    }

    public static IReadOnlyCollection<Diagnostic> WarningsAndErrors(this IEnumerable<Diagnostic> diagnostics) =>
        diagnostics
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .ToList();
}