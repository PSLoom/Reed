# PSLoom.Reed

Declarative argument completion for native commands: describe subcommands, options, arguments, aliases and completion sources with the Sley DSL.

Reed requires PowerShell 7.6+ and the `PSLoom` kernel module. Load the kernel and thread Reed into a draft:

```powershell
Import-Module PSLoom
Invoke-Loom -Draft {
  Thread Reed
  Sley git {
    Command log { Option --oneline; Option --graph }
    Command checkout -Alias co { Argument branch }
  }
}
```

> [!NOTE]
> PowerShell Gallery publishing is not available yet. Once published, installation will use `Install-PSResource PSLoom.Reed` with the `PSLoom` kernel installed as well. For now, follow the [repository build instructions](https://github.com/PSLoom/Reed#build-and-test).

See the [PSLoom documentation](https://github.com/PSLoom/wiki) for the DSL, providers, caching and JSON portability.
