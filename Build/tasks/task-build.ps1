[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Path,

    [Parameter()]
    [ValidateSet("Release", "Debug")]
    [string]
    $Configuration = "Release"
)

task Default DotnetRestore, DotnetBuild

task DotnetRestore {
    exec { dotnet restore $Path }
}

task DotnetBuild {
    exec {
        dotnet build $Path `
            -t:Rebuild `
            -p:Configuration=$Configuration `
            -p:ContinuousIntegrationBuild=true `
            -p:EmbedUntrackedSources=true
    }
}
