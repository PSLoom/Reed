// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Model;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   What Reed's verbs and cmdlets share: reaching the session's engine, and registering a completer.
/// </summary>
internal static class ReedEngine {
  /// <summary>
  ///   Gets the engine intrinsics of the session running a cmdlet.
  /// </summary>
  public static EngineIntrinsics Of(PSCmdlet cmdlet) {
    ArgumentNullException.ThrowIfNull(cmdlet);

    return cmdlet.GetVariableValue("ExecutionContext") as EngineIntrinsics ??
           throw ReedException.InvalidDeclaration("The session's engine is unavailable.", cmdlet.MyInvocation.MyCommand?.Name);
  }

  /// <summary>
  ///   Registers a completer in the current runspace and wires its names into tab completion.
  /// </summary>
  /// <exception cref="ReedException">A name is taken, or wiring failed.</exception>
  public static CompleterRegistration Register(EngineIntrinsics engine, CompleterDefinition definition, bool force) {
    var session = ReedSession.ForCurrent();
    var registration = session.Completers.Add(definition, force);

    CompleterWiring.Ensure(engine, session, registration);

    return registration;
  }
}
