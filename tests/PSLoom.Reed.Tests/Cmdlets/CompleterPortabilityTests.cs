// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Cmdlets;
using PSLoom.Reed.Serialization;
using PSLoom.Reed.Tests.Utility;

namespace PSLoom.Reed.Tests.Cmdlets;

[TestSubject(typeof(ExportCompleterCmdlet))]
[TestSubject(typeof(ImportCompleterCmdlet))]
public sealed class CompleterPortabilityTests : IDisposable {
  private readonly string _directory = Directory.CreateTempSubdirectory("psloom-reed-").FullName;

  public void Dispose()
    => Directory.Delete(_directory, true);

  [Fact]
  public void ACompleterRoundTripsThroughAFile() {
    var file = Path.Combine(_directory, "git.json");

    using (var author = new CompletionSession()) {
      author.Run(
        $$"""
          Invoke-Loom {
            Thread Reed
            Sley git -Alias g -Description 'content tracker' {
              Option --version
              Command commit -Alias ci -Description 'Record changes' {
                Option --message -Alias '-m' { Argument text }
                Argument files -Variadic -Provider git-files
              }
            }
          }
          Export-Completer git '{{file}}'
          """);

      author.Streams.Error.ShouldBeEmpty();
    }

    using var reader = new CompletionSession();
    reader.Run("Invoke-Loom { Thread Reed }");
    reader.Run($"Import-Completer '{file}' | Register-Completer");

    reader.Streams.Error.ShouldBeEmpty();
    reader.Run("(Get-Completer git).Names").Select(result => (string)result.BaseObject).ShouldBe(["git", "g"]);
    reader.Complete("git co").ShouldBe(["commit"]);
    reader.Complete("g ci --").ShouldBe(["--message"]);

    // The provider name travelled; it completes as soon as this session registers one under that name.
    reader.Run("Register-CompletionProvider git-files { 'README.md' }");
    reader.Complete("git commit R").ShouldBe(["README.md"]);
  }

  [Fact]
  public void AnInlineSourceBlocksTheExportUntilForced() {
    var file = Path.Combine(_directory, "inline.json");
    using var session = new CompletionSession();
    session.Run("Invoke-Loom { Thread Reed; Sley git { Command switch { Argument branch -Source { 'main' } } } }");

    session.Run($"Export-Completer git '{file}'");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.COMPLETER_NOT_PORTABLE);
    session.Streams.Error.ShouldHaveSingleItem().Exception.Message.ShouldContain("git/switch/branch");
    File.Exists(file).ShouldBeFalse();
    session.Streams.Error.Clear();

    session.Run($"Export-Completer git '{file}' -Force");

    session.Streams.Error.ShouldBeEmpty();
    session.Streams.Warning.ShouldHaveSingleItem().Message.ShouldContain("git/switch/branch");
    File.Exists(file).ShouldBeTrue();
  }

  [Fact]
  public void ExportingAnUnknownCommandIsAnError() {
    using var session = new CompletionSession();

    session.Run($"Export-Completer nope '{Path.Combine(_directory, "nope.json")}'");

    session.Streams.Error.ShouldHaveSingleItem().FullyQualifiedErrorId.ShouldStartWith(ReedException.COMPLETER_NOT_FOUND);
  }

  [Fact]
  public void ACompleterFromTheePipelineIsExportedWithoutRegistering() {
    var file = Path.Combine(_directory, "hub.json");
    using var session = new CompletionSession();
    session.Run("Invoke-Loom { Thread Reed }");

    session.Run($"New-Completer hub -ScriptBlock {{ Command pr {{ }} }} | Export-Completer -Path '{file}' -PassThru");

    session.Streams.Error.ShouldBeEmpty();
    session.Run("Get-Completer").ShouldBeEmpty();
    File.ReadAllText(file).ShouldContain("\"command\": \"hub\"");
  }

  [Theory]
  [InlineData("{ not json", "invalid")]
  [InlineData("""{ "schema": 99, "command": "git" }""", "schema 99")]
  [InlineData("""{ "schema": 1 }""", "names no command")]
  public void AnUnreadableFileIsReported(string content, string expected) {
    var file = Path.Combine(_directory, $"broken{expected.GetHashCode(StringComparison.Ordinal)}.json");
    File.WriteAllText(file, content);
    using var session = new CompletionSession();

    session.Run($"Import-Completer '{file}'");

    var error = session.Streams.Error.ShouldHaveSingleItem();
    error.FullyQualifiedErrorId.ShouldStartWith(ReedException.COMPLETER_NOT_READABLE);

    if (!expected.Equals("invalid", StringComparison.Ordinal)) {
      error.Exception.Message.ShouldContain(expected);
    }
  }

  [Fact]
  public void TheDocumentCarriesItsSchema() {
    var file = Path.Combine(_directory, "schema.json");
    using var session = new CompletionSession();
    session.Run("Invoke-Loom { Thread Reed }");

    session.Run($"New-Completer git -ScriptBlock {{ Command commit {{ }} }} | Export-Completer -Path '{file}'");

    File.ReadAllText(file).ShouldContain($"\"schema\": {CompleterDocument.CURRENT_SCHEMA}");
  }
}
