// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using JetBrains.Annotations;
using PSLoom.Reed.Model;

namespace PSLoom.Reed.Tests.Model;

[TestSubject(typeof(CompleterValidator))]
public sealed class CompleterValidatorTests {
  [Fact]
  public void AWellFormedCompleterHasNoProblems() {
    var definition = Completer(root => {
      var commit = new CommandNode("commit", ["ci"], null);
      commit.Options.Add(new OptionNode("--message", ["-m"], null));
      commit.Arguments.Add(new ArgumentNode("files", true));
      root.Commands.Add(commit);
    });

    CompleterValidator.Validate(definition).ShouldBeEmpty();
  }

  [Fact]
  public void TwoSubcommandsCannotShareASpelling() {
    var definition = Completer(root => {
      root.Commands.Add(new CommandNode("commit", [], null));
      root.Commands.Add(new CommandNode("checkout", ["commit"], null));
    });

    var problem = CompleterValidator.Validate(definition).ShouldHaveSingleItem();
    problem.IsError.ShouldBeTrue();
    problem.Path.ShouldBe("git/commit");
    problem.Message.ShouldContain("'commit' is declared by both subcommands 'commit' and 'checkout'");
  }

  [Fact]
  public void TwoOptionsCannotShareAnAlias() {
    var definition = Completer(root => {
      root.Options.Add(new OptionNode("--message", ["-m"], null));
      root.Options.Add(new OptionNode("--minimal", ["-m"], null));
    });

    CompleterValidator.Validate(definition).ShouldHaveSingleItem().Message.ShouldContain("'-m' is declared by both options");
  }

  [Fact]
  public void AnOptionCannotRepeatItsOwnName() {
    var definition = Completer(root => root.Options.Add(new OptionNode("--message", ["--message"], null)));

    CompleterValidator.Validate(definition).ShouldHaveSingleItem().Message.ShouldContain("declares '--message' twice");
  }

  [Fact]
  public void NothingMayFollowAVariadicArgument() {
    var definition = Completer(root => {
      root.Arguments.Add(new ArgumentNode("files", true));
      root.Arguments.Add(new ArgumentNode("extra", false));
    });

    CompleterValidator.Validate(definition).ShouldHaveSingleItem().Path.ShouldBe("git/files");
  }

  [Fact]
  public void AnEmptyNameIsReported() {
    var definition = Completer(root => root.Options.Add(new OptionNode(" ", [], null)));

    CompleterValidator.Validate(definition).ShouldHaveSingleItem().Message.ShouldContain("empty name");
  }

  [Fact]
  public void NestedCommandsAreCheckedToo() {
    var definition = Completer(root => {
      var remote = new CommandNode("remote", [], null);
      remote.Commands.Add(new CommandNode("add", [], null));
      remote.Commands.Add(new CommandNode("add", [], null));
      root.Commands.Add(remote);
    });

    CompleterValidator.Validate(definition).ShouldHaveSingleItem().Path.ShouldBe("git/remote/add");
  }

  private static CompleterDefinition Completer(Action<CommandNode> declare) {
    var definition = new CompleterDefinition("git", [], null);
    declare(definition.Root);

    return definition;
  }
}
