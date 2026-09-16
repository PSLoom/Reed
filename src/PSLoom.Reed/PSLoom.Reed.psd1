@{
  RootModule = 'PSLoom.Reed.dll'
  ModuleVersion = '$version$'
  CompatiblePSEditions = @('Core')
  GUID = '5b0c8f3e-9a61-4d2e-b7c4-3f1e8a2d6c90'

  Author = 'Bruno Sales'
  Copyright = '(c) 2026 Bruno Sales <me@baliestri.dev>. All rights reserved.'
  Description = 'Declarative argument completion for native commands - a PSLoom harness'

  PowerShellVersion = '7.6'

  RequiredModules = @('PSLoom')

  FunctionsToExport = @()
  VariablesToExport = @()
  AliasesToExport = @()

  CmdletsToExport = @(
    'New-Completer', 'Register-Completer', 'Get-Completer', 'Unregister-Completer',
    'Test-Completer', 'Export-Completer', 'Import-Completer', 'Trace-Completion',
    'Register-CompletionProvider', 'Get-CompletionProvider', 'Unregister-CompletionProvider',
    'Clear-CompletionCache'
  )

  PrivateData = @{
    PSData = @{
      Tags = @('powershell', 'pwsh', 'psloom', 'harness', 'completion')
      LicenseUri = 'https://github.com/baliestri/PSLoom/blob/main/LICENSE.md'
      ProjectUri = 'https://github.com/baliestri/PSLoom'
    }
  }
}
