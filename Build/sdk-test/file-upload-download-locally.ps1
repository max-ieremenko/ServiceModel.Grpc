[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

task Default Clean, Build, Run

task Clean {
    Remove-DirectoryRecurse -Path (Join-Path $settings.examples "FileUploadDownload") -Filters "bin", "obj"
}

task Build {
    Build-ExampleInContainer `
        -Sources $settings.sources `
        -Examples $settings.examples `
        -Packages $settings.buildOut `
        -ExampleName "FileUploadDownload" `
        -Configuration "Debug" `
        -DotNet "net6.0"
}

task Run {
    Invoke-ExampleInContainer `
        -Example (Join-Path $settings.examples "FileUploadDownload") `
        -DotNet "net6.0" `
        -Configuration "Debug" `
        -Apps "Benchmarks"
}