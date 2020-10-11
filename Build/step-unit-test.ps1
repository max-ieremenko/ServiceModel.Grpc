[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("net461", "netcoreapp2.1", "netcoreapp3.1", "net5.0")] 
    [string]
    $Framework
)

$sourceDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))

$testList = Get-ChildItem -Path $sourceDir -Recurse -Filter *.Test.dll `
    | Where-Object FullName -Match \\$Framework\\ `
    | Where-Object FullName -Match \\bin\\Release\\ `
    | Where-Object FullName -NotMatch \\$Framework\\ref\\ `
    | ForEach-Object {$_.FullName}

if (-not $testList.Count) {
    throw ($Framework + " test list is empty.")
}

$testList
dotnet vstest $testList
