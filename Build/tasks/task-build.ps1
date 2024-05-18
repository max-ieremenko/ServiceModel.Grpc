[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Path,

    [Parameter()]
    [ValidateSet("Release", "Debug")]
    [string]
    $Configuration = "Release",

    [Parameter()]
    [ValidateSet("Rebuild", "Publish")]
    [string]
    $Mode = "Rebuild"
)

task Default DotnetRestore, DotnetBuild, DotnetPublish

task DotnetRestore {
    exec { dotnet restore $Path }
}

task DotnetBuild -If ($Mode -eq "Rebuild") {
    exec {
        dotnet build $Path `
            -t:Rebuild `
            -p:Configuration=$Configuration `
            -p:ContinuousIntegrationBuild=true `
            -p:EmbedUntrackedSources=true
    }
}

task DotnetPublish -If ($Mode -eq "Publish") {
    exec {
        dotnet publish $Path `
            --configuration $Configuration
    }
}