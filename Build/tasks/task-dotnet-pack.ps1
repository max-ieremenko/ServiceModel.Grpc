[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $ProjectFile,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $BuildOut
)

task . {
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -o $BuildOut `
            $ProjectFile
    }
}