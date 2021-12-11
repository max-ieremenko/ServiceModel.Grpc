[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

task Default Clean, Build, Run

task Clean {
    Remove-DirectoryRecurse -Path (Join-Path $settings.examples "ServerFilters") -Filters "bin", "obj"
}

task Build {
    Build-ExampleInContainer `
        -Sources $settings.sources `
        -Examples $settings.examples `
        -Packages $settings.buildOut `
        -ExampleName "ServerFilters" `
        -DotNet "net5.0"
}

task Run {
    Invoke-ExampleInContainer `
        -Example (Join-Path $settings.examples "ServerFilters") `
        -DotNet "net5.0" `
        -Apps "ServerAspNetHost", "ServerSelfHost"
}