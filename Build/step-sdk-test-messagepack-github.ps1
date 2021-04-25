$packageDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../build-out"))
$examplesDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../Examples/MessagePackMarshaller"))

Write-Host "=== add package source ==="
Exec { dotnet nuget add source $packageDir }

Write-Host "=== restore ==="
Exec { dotnet restore $examplesDir }

Write-Host "=== build ==="
Exec { dotnet build --configuration Release $examplesDir }

$apps = @("Demo.ServerAspNetCore", "Demo.ServerSelfHost")
foreach ($app in $apps) {
    Write-Host "=== exec $app ==="
    $entryPoint = Join-Path $examplesDir "$app/bin/Release/net5.0/$app.dll"
    Exec { dotnet $entryPoint }
}
