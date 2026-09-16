// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSLoom.TestKit;
using PSLoom.Warp.Hosting;

namespace PSLoom.Reed.Tests.Utility;

/// <summary>
///   A runspace with the kernel and Reed composed, and a pipeline on it. The harness is registered directly against the runspace
///   instead of importing the published module, so the test process keeps a single copy of every assembly.
/// </summary>
internal sealed class CompletionSession : IDisposable {
  public CompletionSession() {
    Runspace = PowerShellHost.CreateRunspace(state => {
      state.AddCmdletsFrom(typeof(ModuleInitializer).Assembly);
      state.AddCmdletsFrom(typeof(ReedHarness).Assembly);
    });

    using (PowerShellHost.UseAsDefault(Runspace)) {
      // What Import-Module PSLoom does: attach the kernel and hand it the engine. Then the harness registers itself, exactly as
      // Import-Module PSLoom.Reed would.
      new ModuleInitializer().OnImport();
      HarnessHost.Register<ReedHarness>();
    }

    Shell = PowerShellHost.CreateShell(Runspace);
  }

  public Runspace Runspace { get; }

  public PowerShell Shell { get; }

  public PSDataStreams Streams => Shell.Streams;

  public Collection<PSObject> Run(string script)
    => Shell.Run(script);

  /// <summary>
  ///   Completes an input line at its end, the way a Tab press does, and returns the completion texts.
  /// </summary>
  public IReadOnlyList<string> Complete(string input) {
    var results = Run($"(TabExpansion2 -inputScript {Quote(input)} -cursorColumn {input.Length}).CompletionMatches");

    return [.. results.Select(result => (string)result.Properties["CompletionText"].Value)];
  }

  public void Dispose() {
    Shell.Dispose();
    Runspace.Dispose();
  }

  private static string Quote(string value)
    => $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";
}
