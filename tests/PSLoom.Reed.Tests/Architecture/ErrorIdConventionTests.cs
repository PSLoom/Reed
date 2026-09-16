// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using PSLoom.TestKit;

namespace PSLoom.Reed.Tests.Architecture;

public sealed class ErrorIdConventionTests {
  [Fact]
  public void ReedIdsUseOnlyTheReedPrefix()
    => ErrorIdConvention.Violations(typeof(ReedException).Assembly, "REED").ShouldBeEmpty();

  [Fact]
  public void TheConventionFindsTheIds()
    => ErrorIdConvention.Ids(typeof(ReedException).Assembly).ShouldContain(entry => entry.Id == ReedException.COMPLETER_NAME_TAKEN);
}
