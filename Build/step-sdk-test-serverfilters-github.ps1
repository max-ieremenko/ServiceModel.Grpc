$examplesDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../Examples/ServerFilters"))

Write-Host "=== restore ==="
Exec { dotnet restore $examplesDir }

Write-Host "=== build ==="
Exec { dotnet build --configuration Release $examplesDir }

$apps = @("ServerAspNetHost", "ServerSelfHost")
foreach ($app in $apps) {
    Write-Host "=== exec $app ==="
    $entryPoint = Join-Path $examplesDir "$app/bin/Release/net5.0/$app.dll"
    Exec { dotnet $entryPoint }
}
