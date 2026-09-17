# Reed

Declarative argument completion for native commands in PowerShell. Reed is a [PSLoom](https://github.com/PSLoom/PSLoom) harness: the `PSLoom` module supplies the kernel and must be loaded first.

Describe commands, options, arguments and aliases once, then use them through PowerShell's normal Tab completion. Reed also supports dynamic sources, completion providers, caching and JSON import/export.

```powershell
Import-Module PSLoom
Invoke-Loom -Draft {
  Thread Reed
  Sley git -Alias g -Description 'Distributed version control' {
    OptionGroup Common {
      Option --verbose -Alias '-v'
      Option --quiet -Alias '-q'
    }

    Option --version
    Option --help -Alias '-h'

    Command commit -Alias ci -Description 'Record changes' {
      Use OptionGroup Common
      Option --message -Alias '-m' { Argument text }
      Option --amend
      Argument files -Variadic
    }

    Command checkout -Alias co -Description 'Switch branches' {
      Use OptionGroup Common
      Option --branch -Alias '-b' { Argument name }
      Argument target
    }

    Command log -Description 'Show commit logs' {
      Option --oneline
      Option --graph
      Option --max-count -Alias '-n' { Argument count }
    }

    Command remote -Description 'Manage remotes' {
      Command add { Argument name; Argument url }
      Command remove -Alias rm { Argument name }
    }
  }
}
```

Try `git co<Tab>`, `g ci --m<Tab>` or `git remote <Tab>`. See the [central documentation](https://github.com/PSLoom/wiki) for the Sley DSL and provider reference.

## Build and test

Use the .NET SDK selected by `global.json` (10.0.400 or a compatible later feature band), PowerShell 7.6+ and Git. Set `GITHUB_PACKAGES_TOKEN` in your environment to a personal access token with `read:packages` and access to the private PSLoom packages.

The repository split lives on `develop`; `main` remains at its initial commit. Clone the development branch:

```powershell
git clone --branch develop https://github.com/PSLoom/Reed.git
Set-Location Reed
```

```powershell
dotnet build PSLoom.Reed.slnx -c Release
dotnet test --solution PSLoom.Reed.slnx -c Release --no-build
```

The solution restores `PSLoom.Build`, `PSLoom.Warp`, `PSLoom.TestKit` and `PSLoom` at one `PSLoomVersion`, pinned in `Directory.Packages.props`. The integration and benchmark projects restore the kernel module into `artifacts/modules/PSLoom`; the Reed build publishes beside it in `artifacts/modules/PSLoom.Reed`. Only the kernel module ships `Warp.dll`.

To try the built modules in a fresh PowerShell session:

```powershell
$env:PSModulePath = (Join-Path $PWD 'artifacts/modules') + [IO.Path]::PathSeparator + $env:PSModulePath
Import-Module PSLoom
Invoke-Loom -Draft { Thread Reed }
```

CI uses `GITHUB_TOKEN` with `packages: read`; the Reed repository also needs Actions read access on each private kernel package. Builds run on Windows, Linux and macOS for pushes to `main`, `develop` and `reed/v*` tags, and for pull requests.

## Local kernel development

In the [kernel repository](https://github.com/PSLoom/PSLoom), run `./build/Pack-Local.ps1`. It writes all four packages to `~/.psloom/packages` and prints a version such as `0.0.0-local.20260916120000`. Back here, use the exact printed version:

```powershell
$kernelVersion = '0.0.0-local.20260916120000' # replace with the printed version
dotnet build PSLoom.Reed.slnx -c Release -p:PSLoomVersion=$kernelVersion
dotnet test --solution PSLoom.Reed.slnx -c Release --no-build -p:PSLoomVersion=$kernelVersion
dotnet pack src/PSLoom.Reed -c Release -o artifacts/packages -p:PSLoomVersion=$kernelVersion
```

Release restores use `nuget.config`: source mapping routes `PSLoom` and `PSLoom.*` to GitHub Packages and all other packages to nuget.org. This makes central package management unambiguous without suppressing restore warnings.

For a `0.0.0-local.` version, `Directory.Build.props` selects `nuget.local.config` through `RestoreConfigFile` and replaces `RestoreSources` with the local folder and nuget.org. The local config has no package mappings or private feed credentials, so a dynamic local folder works without inheriting the release mapping. No GitHub token is required for this loop; public dependencies still need nuget.org or a populated cache. An additional source alone would still allow queries to the private feed. If the packages are elsewhere, also pass `-p:PSLoomLocalPackageSource=<absolute-folder>`.

`PSLoomVersion` selects the kernel SDK. Reed's own version comes from `reed/v*` tags; `VersionOverride` can override its package version independently.

## Performance checks

Build the whole solution (or the benchmark project alone) first to restore the kernel module and publish Reed, then run:

```powershell
./benchmarks/Measure-Startup.ps1 -DraftPath ./benchmarks/drafts/typical.ps1
./benchmarks/Measure-Startup.ps1 -DraftPath ./benchmarks/drafts/eager.ps1 -ReportOnly
dotnet run -c Release --project benchmarks/PSLoom.Reed.Benchmarks -- --filter '*CompletionBenchmarks*' --exporters json
./benchmarks/Assert-Budgets.ps1
```

When using local packages, set these environment variables before `dotnet run` so BenchmarkDotNet's generated child projects inherit the same SDK selection:

```powershell
$env:PSLoomVersion = $kernelVersion
# Only for packages outside ~/.psloom/packages:
# $env:PSLoomLocalPackageSource = 'D:\psloom\repos\PSLoom\artifacts\packages'
```

Remove these environment variables when returning to the pinned release SDK. Add `--job short` after `--` in the benchmark command for a shorter local run.

Completion budgets are under 5 ms per Tab operation. Startup gates measure cold-process medians: kernel import under 50 ms and the typical Reed draft under 150 ms, with the scripts' default 1.2 time tolerance. The eager draft is reported without gating.

Reed is distributed under the [MIT license](LICENSE.md).
