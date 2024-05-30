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

task Default {
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -o $BuildOut `
            $ProjectFile
    }
}