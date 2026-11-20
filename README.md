# Reed

Describe the arguments of a command **once** and get tab completion for it in PowerShell. Reed is the argument
completion harness of [PSLoom](https://github.com/PSLoom/PSLoom): it installs on top of the base module and adds
completion without touching the rest of your session.

> [!NOTE]
> This repository does not complete anything yet. It currently proves that a harness builds, tests and packages
> against the base; the completion engine comes next. Nothing is published to the PowerShell Gallery.

## Requirements

- PowerShell 7.6 or later (`pwsh`).
- The base module `PSLoom`. Installing `PSLoom.Reed` from the Gallery is meant to bring it along.
- For a source build only: the .NET SDK pinned in [`global.json`](global.json), and the base repository cloned
  next to this one.

## Install

Once a release is published, install from the PowerShell Gallery in PowerShell 7:

```powershell
Install-PSResource PSLoom.Reed
```

This command does not work yet: no version has been published. Use the [source build](#build-from-source)
meanwhile.

## Use it

Import the module and confirm that the base sees it:

```powershell
Import-Module PSLoom.Reed
Get-Harness
```

`Import-Module` loads the base first. `Get-Harness`, a command of the base, then lists `PSLoom.Reed` with the
contract version it was built against.

## Troubleshooting

| What you see                                                 | What to check                                                                                                        |
| :----------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------- |
| `Import-Module` says the required module `PSLoom` is missing | The base is not installed, or is older than the version this harness requires. Install or update `PSLoom`.           |
| The harness reports another contract version                 | The harness and the base come from incompatible releases. Update the one that is behind.                             |
| The build says the base repository was not found             | Clone `PSLoom/PSLoom` next to this repository, in a folder named `PSLoom`, at the ref in [`PSLoom.ref`](PSLoom.ref). |

## Build from source

Clone the base next to this repository, then build and open a session that sees both modules:

```powershell
git clone https://github.com/PSLoom/PSLoom.git ../PSLoom
git -C ../PSLoom checkout (Get-Content PSLoom.ref)
dotnet build
pwsh -NoProfile -File ../PSLoom/eng/Enter-DevShell.ps1
```

Building this repository builds the base as well. The modules are assembled in `artifacts/modules/` of each
repository.

Tests, validation scripts, and repository conventions are in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Reed is licensed under the [zlib/libpng License](LICENSE.md).
