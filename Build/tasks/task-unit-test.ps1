[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Sources,

    [Parameter(Mandatory)]
    [ValidateSet("net461", "netcoreapp3.1", "net5.0", "net6.0", "net7.0")] 
    [string]
    $Framework
)

task Default {
    $testList = Get-ChildItem -Path $Sources -Recurse -Filter *.Test.dll `
    | Where-Object FullName -Match \\$Framework\\ `
    | Where-Object FullName -Match \\bin\\Release\\ `
    | Where-Object FullName -NotMatch \\$Framework\\ref\\ `
    | ForEach-Object { $_.FullName }
    
    assert $testList.Count "$Framework test list is empty"
    
    $testList
    exec { dotnet vstest $testList }
}
