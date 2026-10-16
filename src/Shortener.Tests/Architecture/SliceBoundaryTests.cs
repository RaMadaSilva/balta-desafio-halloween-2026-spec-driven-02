using System.Text.RegularExpressions;

namespace Shortener.Tests.Architecture;

public sealed class SliceBoundaryTests
{
    private static readonly string[] SliceNames = ["Create", "Redirect", "Stats"];

    [Fact]
    public void ProductionSlicesDoNotReferenceOtherSlices()
    {
        var linksDirectory = FindLinksDirectory();
        var violations = new List<string>();

        foreach (var currentSlice in SliceNames)
        {
            var sliceDirectory = Path.Combine(linksDirectory, currentSlice);
            Assert.True(Directory.Exists(sliceDirectory), $"Slice directory does not exist: {sliceDirectory}");

            foreach (var sourceFile in Directory.EnumerateFiles(sliceDirectory, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(sourceFile);
                foreach (var otherSlice in SliceNames.Where(name => name != currentSlice))
                {
                    if (Regex.IsMatch(source, $@"\bShortener\.Api\.Features\.Links\.{Regex.Escape(otherSlice)}\b"))
                    {
                        violations.Add($"{Path.GetRelativePath(linksDirectory, sourceFile)} references {otherSlice}");
                    }
                }
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    private static string FindLinksDirectory()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDirectory is not null)
        {
            var linksDirectory = Path.Combine(currentDirectory.FullName, "Shortener.Api", "Features", "Links");
            if (Directory.Exists(linksDirectory))
            {
                return linksDirectory;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Shortener.Api/Features/Links.");
    }
}
