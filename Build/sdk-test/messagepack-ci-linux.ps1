[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

Enter-Build {
    $exampleDir = Join-Path $Settings.examples "MessagePackMarshaller"
}

task Default Build, Run

task Build {
    exec { dotnet restore $exampleDir }
    exec { dotnet build --configuration Release $exampleDir }
}

task Run {
    $apps = @("Demo.ServerAspNetCore", "Demo.ServerSelfHost")
    foreach ($app in $apps) {
        Write-Output "=== exec $app ==="

        $entryPoint = Join-Path $exampleDir "$app/bin/Release/net5.0/$app.dll"
        exec { dotnet $entryPoint }
    }
}
