// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;

namespace PSLoom.Reed.Runtime;

/// <summary>
///   The completion providers registered in one runspace.
/// </summary>
internal sealed class ProviderRegistry {
  private readonly Lock _lock = new();
  private readonly Dictionary<string, CompletionProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  ///   Gets every provider, by name.
  /// </summary>
  public IReadOnlyList<CompletionProvider> All {
    get {
      lock (_lock) {
        return [.. _providers.Values];
      }
    }
  }

  /// <summary>
  ///   Registers a provider.
  /// </summary>
  /// <exception cref="ReedException">The name is taken and <paramref name="force" /> is <see langword="false" />.</exception>
  public CompletionProvider Add(string name, ScriptBlock scriptBlock, string? description, bool force) {
    ArgumentNullException.ThrowIfNull(name);
    ArgumentNullException.ThrowIfNull(scriptBlock);

    var provider = new CompletionProvider(name, scriptBlock, description);

    lock (_lock) {
      if (!force &&
          _providers.ContainsKey(name)) {
        throw ReedException.ProviderNameTaken(name);
      }

      _providers[name] = provider;
    }

    return provider;
  }

  public bool TryGet(string name, [NotNullWhen(true)] out CompletionProvider? provider) {
    ArgumentNullException.ThrowIfNull(name);

    lock (_lock) {
      return _providers.TryGetValue(name, out provider);
    }
  }

  /// <summary>
  ///   Removes a provider.
  /// </summary>
  /// <returns>The removed provider, or <see langword="null" /> when the name was not registered.</returns>
  public CompletionProvider? Remove(string name) {
    ArgumentNullException.ThrowIfNull(name);

    lock (_lock) {
      return _providers.Remove(name, out var provider) ? provider : null;
    }
  }
}
