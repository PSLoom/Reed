// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the zlib/libpng License.
// See the LICENSE file in the repository root for full license text.

using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using PSLoom.TestKit;

namespace PSLoom.Reed.Tests;

/// <summary>Exercises the built module the way an installed one is used: in a real pwsh, next to the base.</summary>
public sealed class ModuleTests {
  private readonly static string Root = RepoLayout.FindRoot();
  private readonly static string BaseRoot = Path.GetFullPath(Path.Combine(Root, "..", "PSLoom"));
  private readonly static IReadOnlyList<string> ModulePaths = RepoLayout.ModulePaths(Root);

  [Fact]
  public void ImportLoadsTheBaseFirstAndRegistersTheHarness() {
    const string SCRIPT = """
      $before = @(Get-Module).Name
      Import-Module PSLoom.Reed
      [pscustomobject]@{
        Added          = @(@(Get-Module).Name | Where-Object { $_ -notin $before } | Sort-Object)
        Harnesses      = @(Get-Harness | ForEach-Object { $_.Name })
        BaseAssemblies = @([System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'PSLoom' } | ForEach-Object { $_.Location })
      } | ConvertTo-Json -Compress
      """;

    var result = PwshProcess.Run(SCRIPT, ModulePaths);

    Assert.Equal(0, result.ExitCode);

    using var document = JsonDocument.Parse(result.StandardOutput);
    var root = document.RootElement;

    Assert.Equal(new[] { "PSLoom", "PSLoom.Reed" }, Strings(root, "Added"));
    Assert.Equal(new[] { "PSLoom.Reed" }, Strings(root, "Harnesses"));
    Assert.StartsWith(RepoLayout.ModulesDirectory(BaseRoot), Assert.Single(Strings(root, "BaseAssemblies")),
      StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void TheManifestRequiresTheBaseAtTheVersionItWasBuiltAgainst() {
    var baseVersion = Regex.Match(File.ReadAllText(Path.Combine(BaseRoot, "Version.props")),
      "<Version>(?<version>[0-9.]+)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant,
      TimeSpan.FromSeconds(1));

    var result =
      PwshProcess.Run(
        "(Test-ModuleManifest -Path (Get-Module -ListAvailable PSLoom.Reed)[0].Path -ErrorAction Stop).RequiredModules | ForEach-Object { \"$($_.Name)=$($_.Version)\" }",
        ModulePaths);

    Assert.Equal(0, result.ExitCode);
    Assert.Equal($"PSLoom={baseVersion.Groups["version"].Value}", result.StandardOutput.Trim());
  }

  [Fact]
  public void TheModuleFolderHoldsNoCopyOfTheBase() {
    var files = Directory.GetFiles(Path.Combine(RepoLayout.ModulesDirectory(Root), "PSLoom.Reed"))
      .Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray();

    Assert.Equal(new[] { "PSLoom.Reed.dll", "PSLoom.Reed.psd1" }, files);
  }

  [Fact]
  public void ThePackageDeclaresTheBaseAsADependency() {
    var directory = Directory.CreateTempSubdirectory("psloom-reed-package-");

    try {
      var result = PwshProcess.RunFile(Path.Combine(BaseRoot, "eng", "New-ModulePackage.ps1"),
        ["-Path", Root, "-Destination", directory.FullName]);

      Assert.Equal(0, result.ExitCode);

      using var archive =
        ZipFile.OpenRead(Assert.Single(Directory.GetFiles(directory.FullName, "PSLoom.Reed.*.nupkg")));
      using var reader = new StreamReader(archive.GetEntry("PSLoom.Reed.nuspec")!.Open());

      Assert.Contains("<dependency id=\"PSLoom\"", reader.ReadToEnd(), StringComparison.Ordinal);
    }
    finally {
      directory.Delete(true);
    }
  }

  [Fact]
  public void TheSharedFilesAreInSync() {
    var result = PwshProcess.RunFile(Path.Combine(BaseRoot, "eng", "Sync-Common.ps1"), ["-Path", Root, "-Check"]);

    Assert.True(result.ExitCode == 0, result.StandardOutput);
  }

  private static string?[] Strings(JsonElement element, string property)
    => element.GetProperty(property).EnumerateArray().Select(item => item.GetString()).ToArray();
}
