// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Warp.Hosting;

namespace PSLoom.Reed;

/// <summary>
///   Registers the harness with the runspace importing this module. Nothing scans for harnesses; a harness registers itself.
/// </summary>
public sealed class ReedModuleInitializer : IModuleAssemblyInitializer {
  /// <inheritdoc />
  public void OnImport()
    => HarnessHost.Register<ReedHarness>();
}
