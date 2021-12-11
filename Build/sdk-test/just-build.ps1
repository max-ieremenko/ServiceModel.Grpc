[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings,

    [Parameter(Mandatory = $true)]
    $Example
)

task Default {
    $exampleDir = Join-Path $Settings.examples $Example

    exec { dotnet restore $exampleDir }
    exec { dotnet build --configuration Release $exampleDir }
}
