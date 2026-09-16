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
    "param([string[]]$Name, $ScriptBlock) Register-ArgumentCompleter -Native -CommandName $Name -ScriptBlock $ScriptBlock -ErrorAction Stop";

  /// <summary>
  ///   Ensures every name is wired, with a single call for all the names not wired yet.
  /// </summary>
  /// <exception cref="ReedException">Registering the native completer failed (<c>REED_WIRING_FAILED</c>).</exception>
  public static void Ensure(EngineIntrinsics engine, ReedSession session, IEnumerable<string> names) {
    ArgumentNullException.ThrowIfNull(engine);
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(names);

    var pending = names.Where(name => !session.Wired.Contains(name)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    if (pending.Length == 0) {
      return;
    }

    // Both script blocks are compiled once per runspace: this runs inside a draft, where every millisecond counts against the budget.
    var bridge = session.Bridge ??= ScriptBlock.Create(ReedBridge.SCRIPT);
    var register = session.RegisterScript ??= ScriptBlock.Create(REGISTER_SCRIPT);

    try {
      engine.InvokeCommand.InvokeScript(false, register, null, pending, bridge);
    }
    catch (Exception exception) when (exception is not (OutOfMemoryException or StackOverflowException)) {
      throw ReedException.WiringFailed(string.Join(", ", pending), exception);
    }

    session.Wired.UnionWith(pending);
    session.PendingWiring.ExceptWith(pending);
  }
}
