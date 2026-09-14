// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using System.Xml.Linq;
using PSLoom.TestKit;

namespace PSLoom.Reed.Tests.Architecture;

public sealed class HarnessDependencyTests {
  private const string MODULE_NAME = "PSLoom.Reed";
  private const string REED_PROJECT = "src/PSLoom.Reed/PSLoom.Reed.csproj";

  [Fact]
  public void ReedProjectDoesNotReferenceTheKernel() {
    var includes = ProjectReferences().Select(reference => Path.GetFileName((string)reference.Attribute("Include")!));

    includes.ShouldNotContain("PSLoom.csproj");
  }

  [Fact]
  public void ReedReferencesWarpAtCompileTimeOnly() {
    var warp = ProjectReferences().Single(reference => ((string)reference.Attribute("Include")!).EndsWith("Warp.csproj"));

    ((string?)warp.Attribute("Private")).ShouldBe("false");
    ((string?)warp.Attribute("ExcludeAssets")).ShouldBe("runtime");
  }

  [Fact]
  public void ReedAssemblyDoesNotReferenceTheKernel() {
    var references = Assembly.Load(new AssemblyName(MODULE_NAME)).GetReferencedAssemblies().Select(reference => reference.Name);

    references.ShouldNotContain("PSLoom");
  }

  [Fact]
  public void PublishedReedModuleDoesNotShipItsOwnWarp() {
    var moduleDirectory = RepositoryLayout.GetPublishedModuleDirectory(MODULE_NAME);

    File.Exists(Path.Combine(moduleDirectory, "PSLoom.Reed.dll")).ShouldBeTrue();
    File.Exists(Path.Combine(moduleDirectory, "Warp.dll")).ShouldBeFalse();
    File.Exists(Path.Combine(moduleDirectory, "System.Management.Automation.dll")).ShouldBeFalse();
  }

  [Fact]
  public void PublishedReedManifestRequiresTheKernel() {
    var moduleDirectory = RepositoryLayout.GetPublishedModuleDirectory(MODULE_NAME);
    var manifest = RepositoryLayout.ReadDataFile(Path.Combine(moduleDirectory, "PSLoom.Reed.psd1"));

    ((object[])manifest["RequiredModules"]!).ShouldContain("PSLoom");
  }

  private static IEnumerable<XElement> ProjectReferences()
    => RepositoryLayout.LoadProject(REED_PROJECT).Descendants("ProjectReference");
}
