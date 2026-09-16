// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Runtime;

/// <summary>
///   Wires names into PowerShell's native argument completion. One script block serves every name; the registry decides what a
///   name resolves to, so wiring happens once per name and is never unwound — there is no supported way to remove a native
///   completer from a live session.
/// </summary>
internal static class CompleterWiring {
  private const string REGISTER_SCRIPT =
    "param($Name, $ScriptBlock) Register-ArgumentCompleter -Native -CommandName $Name -ScriptBlock $ScriptBlock -ErrorAction Stop";

  /// <summary>
  ///   Ensures every name is wired.
  /// </summary>
  /// <exception cref="ReedException">Registering the native completer failed (<c>REED_WIRING_FAILED</c>).</exception>
  public static void Ensure(EngineIntrinsics engine, ReedSession session, IEnumerable<string> names) {
    ArgumentNullException.ThrowIfNull(engine);
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(names);

    var bridge = session.Bridge ??= ScriptBlock.Create(ReedBridge.SCRIPT);

    foreach (var name in names) {
      if (!session.Wired.Add(name)) {
        continue;
      }

      try {
        engine.InvokeCommand.InvokeScript(REGISTER_SCRIPT, name, bridge);
      }
      catch (Exception exception) when (exception is not (OutOfMemoryException or StackOverflowException)) {
        session.Wired.Remove(name);
        throw ReedException.WiringFailed(name, exception);
      }
    }
  }
}
