@{
  RootModule = 'PSLoom.Reed.dll'
  ModuleVersion = '$version$'
  GUID = '2d8f47d1-944a-4c43-ad08-46cca4db4f35'
  Author = 'Bruno Sales'
  CompanyName = 'baliestri.dev'
  Copyright = '(c) Bruno Sales <me@baliestri.dev>. Licensed under the zlib/libpng License.'
  Description = 'Declarative argument completion for PowerShell'
  PowerShellVersion = '7.6'
  CompatiblePSEditions = @('Core')
  CmdletsToExport = @($cmdlets$)
  FunctionsToExport = @()
  AliasesToExport = @()
  VariablesToExport = @()
  RequiredModules = @($requiredModules$)
  PrivateData = @{
    PSData = @{
      Tags = @('powershell', 'pwsh', 'completion', 'psloom', 'harness')
      ProjectUri = 'https://github.com/PSLoom/Reed'
      LicenseUri = 'https://github.com/PSLoom/Reed/blob/main/LICENSE.md'
      $prerelease$
    }
  }
}
