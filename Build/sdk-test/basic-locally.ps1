[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

task Default Clean, Build, Run

task Clean {
    Remove-DirectoryRecurse -Path (Join-Path $settings.examples "Basic") -Filters "bin", "obj"
}

task Build {
    Build-ExampleInContainer `
        -Sources $settings.sources `
        -Examples $settings.examples `
        -Packages $settings.buildOut `
        -ExampleName "Basic" `
        -DotNet "net5.0"
}

task Run {
    Invoke-ExampleInContainer `
        -Example (Join-Path $settings.examples "Basic") `
        -DotNet "netcoreapp3.1" `
        -Apps "Demo.AspNet.DesignTime", "Demo.AspNet.ReflectionEmit", "Demo.SelfHost.DesignTime", "Demo.SelfHost.ReflectionEmit"
}
