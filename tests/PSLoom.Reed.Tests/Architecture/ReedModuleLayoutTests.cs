// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation;
using System.Reflection;
using PSLoom.TestKit;

namespace PSLoom.Reed.Tests.Architecture;

public sealed class ReedModuleLayoutTests {
  private const string MODULE_NAME = "PSLoom.Reed";

  [Fact]
  public void PublishedManifestExportsEveryCmdletExactly() {
    var moduleDirectory = RepositoryLayout.GetPublishedModuleDirectory(MODULE_NAME);
    var manifest = RepositoryLayout.ReadDataFile(Path.Combine(moduleDirectory, "PSLoom.Reed.psd1"));
    var exported = ((object[])manifest["CmdletsToExport"]!).Cast<string>().Order(StringComparer.OrdinalIgnoreCase);

    var implemented = typeof(ReedHarness).Assembly.GetTypes()
      .Select(type => type.GetCustomAttribute<CmdletAttribute>())
      .OfType<CmdletAttribute>()
      .Select(cmdlet => $"{cmdlet.VerbName}-{cmdlet.NounName}")
      .Order(StringComparer.OrdinalIgnoreCase);

    exported.ShouldBe(implemented);
  }

  [Fact]
  public void PublishedManifestVersionMatchesItsFolder() {
    var moduleDirectory = RepositoryLayout.GetPublishedModuleDirectory(MODULE_NAME);
    var manifest = RepositoryLayout.ReadDataFile(Path.Combine(moduleDirectory, "PSLoom.Reed.psd1"));

    manifest["ModuleVersion"].ShouldBe(Path.GetFileName(moduleDirectory));
  }
}
