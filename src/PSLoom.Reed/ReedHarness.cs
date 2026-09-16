// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Reed.Verbs;
using PSLoom.Warp.Hosting;

namespace PSLoom.Reed;

/// <summary>
///   Reed: declarative argument completion for native commands. Composing it adds the <c>Sley</c> vocabulary to a draft and gives
///   this runspace's Reed session its trace log.
/// </summary>
[Harness("Reed", Description = "Declarative argument completion")]
public sealed class ReedHarness : IHarness {
  /// <inheritdoc />
  public void Compose(IHarnessBuilder builder) {
    ArgumentNullException.ThrowIfNull(builder);

    builder.Verbs.Add<SleyVerb>();
    builder.Verbs.Add<CommandVerb>();
    builder.Verbs.Add<OptionVerb>();
    builder.Verbs.Add<ArgumentVerb>();
    builder.Verbs.Add<OptionGroupVerb>();
    builder.Verbs.Add<UseVerb>();

    ReedSession.ForCurrent().Traces = builder.Diagnostics.CreateLog<CompletionTrace>();
  }
}
