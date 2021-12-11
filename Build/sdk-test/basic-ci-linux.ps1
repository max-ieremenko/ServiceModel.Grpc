[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

Enter-Build {
    $exampleDir = Join-Path $Settings.examples "Basic"
}

task Default Build, Run

task Build {
    exec { dotnet restore $exampleDir }
    exec { dotnet build --configuration Release $exampleDir }
}

task Run {
    $apps = @("Demo.AspNet.DesignTime", "Demo.AspNet.ReflectionEmit", "Demo.SelfHost.DesignTime", "Demo.SelfHost.ReflectionEmit")
    foreach ($app in $apps) {
        Write-Output "=== exec $app ==="

        $entryPoint = Join-Path $exampleDir "$app/bin/Release/netcoreapp3.1/$app.dll"
        exec { dotnet $entryPoint }
    }
}
