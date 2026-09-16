// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace PSLoom.Reed.Model;

/// <summary>
///   Where an argument's candidates come from.
/// </summary>
public enum CompletionSourceKind {
  /// <summary>
  ///   A script block declared inline, which cannot be exported.
  /// </summary>
  ScriptBlock,

  /// <summary>
  ///   A provider registered by name, which travels with the completer.
  /// </summary>
  Provider
}

/// <summary>
///   What produces the candidates of a value slot, and how they are cached.
/// </summary>
public sealed class CompletionSource {
  internal CompletionSource(ScriptBlock scriptBlock, ScriptBlock? cacheKey) {
    Kind = CompletionSourceKind.ScriptBlock;
    ScriptBlock = scriptBlock;
    CacheKey = cacheKey;
  }

  internal CompletionSource(string providerName, ScriptBlock? cacheKey) {
    Kind = CompletionSourceKind.Provider;
    ProviderName = providerName;
    CacheKey = cacheKey;
  }

  /// <summary>
  ///   Gets the identity of this source, which keys its cache entries. It lives as long as the declaration, so a redeclared
  ///   completer never reads the previous one's cached candidates.
  /// </summary>
  public Guid Id { get; } = Guid.NewGuid();

  /// <summary>
  ///   Gets what produces the candidates.
  /// </summary>
  public CompletionSourceKind Kind { get; }

  /// <summary>
  ///   Gets the inline script block, when there is one.
  /// </summary>
  public ScriptBlock? ScriptBlock { get; }

  /// <summary>
  ///   Gets the provider name, when the source is a named one.
  /// </summary>
  public string? ProviderName { get; }

  /// <summary>
  ///   Gets the script block producing the cache key; <see langword="null" /> means the source runs every time.
  /// </summary>
  public ScriptBlock? CacheKey { get; }

  /// <summary>
  ///   Gets a value indicating whether this source can be written to a file. An inline script block cannot.
  /// </summary>
  public bool IsPortable => Kind == CompletionSourceKind.Provider && CacheKey is null;

  /// <inheritdoc />
  public override string ToString()
    => Kind == CompletionSourceKind.Provider ? $"provider '{ProviderName}'" : "inline source";
}
