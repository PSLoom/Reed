// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using PSLoom.Reed.Model;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   The completers registered in one runspace, indexed by every name each one answers to.
/// </summary>
internal sealed class CompleterRegistry {
  private readonly Dictionary<Guid, CompleterRegistration> _byId = [];
  private readonly Dictionary<string, CompleterRegistration> _byName = new(StringComparer.OrdinalIgnoreCase);
  private readonly Lock _lock = new();

  /// <summary>
  ///   Gets every registration, in registration order.
  /// </summary>
  public IReadOnlyList<CompleterRegistration> All {
    get {
      lock (_lock) {
        return [.. _byId.Values];
      }
    }
  }

  /// <summary>
  ///   Registers a completer.
  /// </summary>
  /// <param name="definition">The declaration.</param>
  /// <param name="force">Replaces whatever registration holds a colliding name.</param>
  /// <returns>The new registration.</returns>
  /// <exception cref="ReedException">A name is already taken and <paramref name="force" /> is <see langword="false" />.</exception>
  public CompleterRegistration Add(CompleterDefinition definition, bool force) {
    ArgumentNullException.ThrowIfNull(definition);

    var registration = new CompleterRegistration(definition);

    lock (_lock) {
      foreach (var name in registration.Names) {
        if (!_byName.TryGetValue(name, out var existing)) {
          continue;
        }

        if (!force) {
          // Name the command actually registered, which is not necessarily the one being registered now: the clash may be on an
          // alias two completers share.
          throw ReedException.CompleterNameTaken(name, existing.Command);
        }

        // Remove the whole colliding registration, every name of it — dropping only the colliding name would leave the rest of
        // that completer answering for a command nobody registered any more.
        RemoveCore(existing);
      }

      _byId[registration.Id] = registration;

      foreach (var name in registration.Names) {
        _byName[name] = registration;
      }
    }

    return registration;
  }

  /// <summary>
  ///   Removes the registration holding a name, with every other name it holds.
  /// </summary>
  /// <returns>The removed registration, or <see langword="null" /> when the name was not registered.</returns>
  public CompleterRegistration? Remove(string name) {
    ArgumentNullException.ThrowIfNull(name);

    lock (_lock) {
      if (!_byName.TryGetValue(name, out var registration)) {
        return null;
      }

      RemoveCore(registration);
      return registration;
    }
  }

  public bool TryGet(string name, [NotNullWhen(true)] out CompleterRegistration? registration) {
    ArgumentNullException.ThrowIfNull(name);

    lock (_lock) {
      return _byName.TryGetValue(name, out registration);
    }
  }

  private void RemoveCore(CompleterRegistration registration) {
    _byId.Remove(registration.Id);

    foreach (var name in registration.Names) {
      if (_byName.TryGetValue(name, out var holder) &&
          holder.Id == registration.Id) {
        _byName.Remove(name);
      }
    }
  }
}
