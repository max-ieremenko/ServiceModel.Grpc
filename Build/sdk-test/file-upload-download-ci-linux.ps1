[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

Enter-Build {
    $exampleDir = Join-Path $Settings.examples "FileUploadDownload"
}

task Default Build, Run

task Build {
    exec { dotnet restore $exampleDir }
    exec { dotnet build --configuration Debug $exampleDir }
}

task Run {
    $entryPoint = Join-Path $exampleDir "Benchmarks/bin/Debug/net6.0/Benchmarks.dll"
    exec { dotnet $entryPoint }
}
