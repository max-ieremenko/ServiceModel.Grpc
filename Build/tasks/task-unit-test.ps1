[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Sources,

    [Parameter(Mandatory)]
    [ValidateSet('net462', 'net6.0', 'net7.0', 'net8.0')] 
    [string]
    $Framework
)

task . {
    $testList = Get-ChildItem -Path $Sources -Recurse -Filter *.Test.dll `
    | Where-Object FullName -Match \\$Framework\\ `
    | Where-Object FullName -Match \\bin\\Release\\ `
    | Where-Object FullName -NotMatch \\$Framework\\ref\\ `
    | ForEach-Object { $_.FullName }
    
    assert $testList.Count "$Framework test list is empty"
    
    $testList
    exec { dotnet vstest $testList }
}
