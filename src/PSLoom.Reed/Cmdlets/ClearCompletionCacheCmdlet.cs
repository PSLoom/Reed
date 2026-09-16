// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.Reed.Runtime;
using PSLoom.Warp;

namespace PSLoom.Reed.Cmdlets;

/// <summary>
///   Forgets every cached source answer, so the next tab press asks the sources again. The way out when a <c>-CacheKey</c> turns
///   out to be coarser than what it describes.
/// </summary>
[Cmdlet(VerbsCommon.Clear, "CompletionCache")]
[OutputType(typeof(void))]
public sealed class ClearCompletionCacheCmdlet : PSCmdlet {
  /// <inheritdoc />
  protected override void ProcessRecord() {
    try {
      var dropped = ReedSession.ForCurrent().Cache.Clear();
      WriteVerbose($"Dropped {dropped} cached completion {(dropped == 1 ? "entry" : "entries")}.");
    }
    catch (PowerShellException exception) {
      WriteError(exception.ToErrorRecord());
    }
  }
}
