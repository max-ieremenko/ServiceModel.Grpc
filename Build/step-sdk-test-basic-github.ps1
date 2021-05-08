$packageDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../build-out"))
$examplesDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../Examples/Basic"))

Write-Host "=== add package source ==="
Exec { dotnet nuget add source $packageDir }

Write-Host "=== restore ==="
Exec { dotnet restore $examplesDir }

Write-Host "=== build ==="
Exec { dotnet build --configuration Release $examplesDir }

$apps = @("Demo.AspNet.DesignTime", "Demo.AspNet.ReflectionEmit", "Demo.SelfHost.DesignTime", "Demo.SelfHost.ReflectionEmit")
foreach ($app in $apps) {
    Write-Host "=== exec $app ==="
    $entryPoint = Join-Path $examplesDir "$app/bin/Release/netcoreapp3.1/$app.dll"
    Exec { dotnet $entryPoint }
}
