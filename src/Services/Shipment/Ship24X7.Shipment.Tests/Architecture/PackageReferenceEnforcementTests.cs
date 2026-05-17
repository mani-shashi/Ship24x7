// Feature: true-microservices-refactor, Property 9: No project contains a ProjectReference to Ship24X7.Shared

using System.Xml.Linq;
using Xunit;

namespace Ship24X7.Shipment.Tests.Architecture;

/// <summary>
/// Fitness function that enforces Property 9: no .csproj file in the solution may contain
/// a &lt;ProjectReference&gt; whose Include path ends with "Ship24X7.Shared.csproj".
/// </summary>
/// <remarks>
/// Validates: Requirements 2.4, 2.5
/// </remarks>
public class PackageReferenceEnforcementTests
{
    /// <summary>
    /// Resolves the solution root by walking up from <see cref="AppContext.BaseDirectory"/>
    /// until a directory containing <c>Ship24X7.sln</c> is found.
    /// Falls back to the <c>SOLUTION_ROOT</c> environment variable when the file cannot be
    /// located by walking (e.g., in some CI environments).
    /// </summary>
    private static string FindSolutionRoot()
    {
        var envOverride = Environment.GetEnvironmentVariable("SOLUTION_ROOT");
        if (!string.IsNullOrWhiteSpace(envOverride) && Directory.Exists(envOverride))
            return envOverride;

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Ship24X7.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the solution root (Ship24X7.sln). " +
            "Set the SOLUTION_ROOT environment variable to the directory containing Ship24X7.sln.");
    }

    [Fact]
    public void NoProject_ShouldHaveProjectReferenceToSharedLibrary()
    {
        // Arrange
        var solutionRoot = FindSolutionRoot();
        var csprojFiles = Directory.GetFiles(solutionRoot, "*.csproj", SearchOption.AllDirectories);

        // Act — collect all violations
        var violations = new List<string>();

        foreach (var csprojPath in csprojFiles)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(csprojPath);
            }
            catch (Exception ex)
            {
                // Skip files that cannot be parsed (e.g., malformed XML during development)
                violations.Add($"Could not parse '{csprojPath}': {ex.Message}");
                continue;
            }

            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

            var offendingReferences = doc
                .Descendants(ns + "ProjectReference")
                .Select(el => el.Attribute("Include")?.Value ?? string.Empty)
                .Where(include =>
                    include.EndsWith("Ship24X7.Shared.csproj", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var reference in offendingReferences)
            {
                violations.Add($"  Project '{csprojPath}' has a ProjectReference to '{reference}'");
            }
        }

        // Assert
        Assert.True(
            violations.Count == 0,
            "One or more projects contain a <ProjectReference> to Ship24X7.Shared.csproj. " +
            "Use <PackageReference Include=\"Ship24X7.Shared\" Version=\"...\"> instead.\n" +
            string.Join("\n", violations));
    }
}
