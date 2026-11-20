// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the zlib/libpng License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace PSLoom.Reed;

/// <summary>Registers the harness with the base when the module is imported.</summary>
[SuppressMessage("ReSharper", "UnusedType.Global",
  Justification = "PowerShell creates it when the module is imported; nothing references it from code.")]
public sealed class ReedModule : IModuleAssemblyInitializer {
  /// <inheritdoc />
  public void OnImport()
    => HarnessRegistry.Register(typeof(ReedModule).Assembly);
}
