#Requires -Version 7.6
param([Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^0\.\d+\.\d+(?:-alpha\.[1-9]\d*)?$') { throw 'Invalid version.' }
$credential = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("PSLoom:$env:GITHUB_PACKAGES_TOKEN"))
$headers = @{ Authorization = "Basic $credential" }
$index = Invoke-RestMethod 'https://nuget.pkg.github.com/PSLoom/index.json' -Headers $headers
$base = ($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'
if (-not $base) { throw 'Missing package download endpoint.' }
foreach ($id in @('psloom','psloom.warp','psloom.build','psloom.testkit')) {
  $uri = "$($base.TrimEnd('/'))/$id/$Version/$id.$Version.nupkg"
  $file = Join-Path $env:RUNNER_TEMP "$id.$Version.nupkg"
  Invoke-WebRequest $uri -Headers $headers -OutFile $file
  $zip = [IO.Compression.ZipFile]::OpenRead($file)
  try { if (-not $zip.Entries.Count) { throw "Empty package: $id" } } finally { $zip.Dispose() }
}
