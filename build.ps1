$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

dotnet build "$root\src\NoBotMod" -c Release -v q --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet build 失败" }

$out = Join-Path $root 'dist\NoBotMod'
New-Item -ItemType Directory -Force $out | Out-Null
Copy-Item "$root\src\NoBotMod\bin\Release\netstandard2.0\NoBotMod.dll" $out -Force
Copy-Item "$root\src\NoBotMod\NoBotMod.json" $out -Force
Write-Host "OK -> $out"
Get-ChildItem $out | Select-Object Name, Length | Format-Table -AutoSize
