// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   What Reed's verbs and cmdlets share: reaching the session's engine, and registering a completer.
/// </summary>
internal static class ReedEngine {
  /// <summary>
  ///   Gets the engine intrinsics of the session running a cmdlet, and remembers them for the paths that have no cmdlet of their
  ///   own — the treadle catalog's change handler.
  /// </summary>
  public static EngineIntrinsics Of(PSCmdlet cmdlet) {
    ArgumentNullException.ThrowIfNull(cmdlet);

    var engine = cmdlet.GetVariableValue("ExecutionContext") as EngineIntrinsics
                 ?? throw ReedException.InvalidDeclaration("The session's engine is unavailable.", cmdlet.MyInvocation.MyCommand?.Name);

    ReedSession.ForCurrent().Engine = engine;

    return engine;
  }

  /// <summary>
  ///   Registers a completer in the current runspace and wires its names into tab completion, plus the treadles that run it.
  /// </summary>
  /// <exception cref="ReedException">A name is taken, or wiring failed.</exception>
  public static CompleterRegistration Register(EngineIntrinsics engine, CompleterDefinition definition, bool force) {
    var session = ReedSession.ForCurrent();
    var replaced = Replaced(session, definition);
    var registration = session.Completers.Add(definition, force);

    // Whatever the replaced declaration cached answered for a tree that no longer exists.
    session.Cache.Remove(replaced);

    CompleterWiring.Ensure(engine, session, registration.Names);
    TreadleCompletion.Wire(engine, session);

    return registration;
  }

  private static IEnumerable<Guid> Replaced(ReedSession session, CompleterDefinition definition)
    => definition.Names
      .Select(name => session.Completers.TryGet(name, out var existing) ? existing : null)
      .OfType<CompleterRegistration>()
      .Distinct()
      .SelectMany(existing => ModelSources.Of(existing.Definition).Select(source => source.Id));
}
