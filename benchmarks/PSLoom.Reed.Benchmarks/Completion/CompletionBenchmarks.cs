// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using BenchmarkDotNet.Attributes;
using PSLoom.Reed;
using PSLoom.Reed.Runtime;
using PSLoom.Warp.Hosting;

namespace PSLoom.Benchmarks.Completion;

/// <summary>
///   One Tab press against the medium completer, through the same entry point PowerShell calls. Budget: under 5 ms.
/// </summary>
[MemoryDiagnoser]
public class CompletionBenchmarks {
  private CommandAst _deep = null!;
  private CommandAst _option = null!;
  private CommandAst _root = null!;
  private Runspace _runspace = null!;

  /// <summary>
  ///   Opens a runspace, composes Reed and registers the fixture.
  /// </summary>
  [GlobalSetup]
  public void Setup() {
    var state = InitialSessionState.CreateDefault2();

    foreach (var type in typeof(ReedHarness).Assembly.GetTypes()) {
      if (type.GetCustomAttributes(typeof(CmdletAttribute), false) is [CmdletAttribute cmdlet]) {
        state.Commands.Add(new SessionStateCmdletEntry($"{cmdlet.VerbName}-{cmdlet.NounName}", type, null));
      }
    }

    _runspace = RunspaceFactory.CreateRunspace(state);
    _runspace.Open();
    Runspace.DefaultRunspace = _runspace;

    new ModuleInitializer().OnImport();
    HarnessHost.Register<ReedHarness>();

    using var shell = PowerShell.Create();
    shell.Runspace = _runspace;
    shell.AddScript(MediumCompleter.Script()).Invoke();

    _root = Command($"{MediumCompleter.COMMAND} comm");
    _deep = Command($"{MediumCompleter.COMMAND} command15 nested2");
    _option = Command($"{MediumCompleter.COMMAND} command15 nested20 --n20-option0");
  }

  /// <summary>
  ///   Closes the runspace.
  /// </summary>
  [GlobalCleanup]
  public void Cleanup()
    => _runspace.Dispose();

  /// <summary>
  ///   Completing a subcommand at the root.
  /// </summary>
  [Benchmark]
  public int Subcommand()
    => ReedBridge.Complete("comm", _root, _root.Extent.EndOffset).Count();

  /// <summary>
  ///   Completing a subcommand two levels down.
  /// </summary>
  [Benchmark]
  public int NestedSubcommand()
    => ReedBridge.Complete("nested2", _deep, _deep.Extent.EndOffset).Count();

  /// <summary>
  ///   Completing an option two levels down.
  /// </summary>
  [Benchmark]
  public int Option()
    => ReedBridge.Complete("--n20-option0", _option, _option.Extent.EndOffset).Count();

  private static CommandAst Command(string input)
    => (CommandAst)Parser.ParseInput(input, out var _, out var _).Find(node => node is CommandAst, true)!;
}
