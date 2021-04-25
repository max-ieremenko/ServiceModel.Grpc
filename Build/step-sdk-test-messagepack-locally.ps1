$packageDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
$examplesDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Examples"))

Get-ChildItem -Recurse -Path (Join-Path $examplesDir "obj") -Directory | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Path (Join-Path $examplesDir "bin") -Directory | Remove-Item -Recurse -Force

$containerId = Exec {
    docker run -d -it -v "${examplesDir}:/examples" -v "${packageDir}:/packages" mcr.microsoft.com/dotnet/sdk:5.0
}

try {
    Write-Host "=== add package source ==="
    Exec { docker exec -it $containerId dotnet nuget add source /packages }

    Write-Host "=== restore ==="
    Exec { docker exec -it $containerId dotnet restore /examples/MessagePackMarshaller }

    Write-Host "=== build ==="
    Exec { docker exec -it $containerId dotnet build --configuration Release /examples/MessagePackMarshaller }
}
finally {
    Exec { docker container rm -f $containerId }
}

$containerId = Exec {
    docker run -d -it -v "${examplesDir}:/examples" mcr.microsoft.com/dotnet/aspnet:5.0
}

try {
    $apps = @("Demo.ServerAspNetCore", "Demo.ServerSelfHost")
    foreach ($app in $apps) {
        Write-Host "=== exec $app ==="
        Exec { docker exec -it $containerId dotnet "/examples/MessagePackMarshaller/$app/bin/Release/net5.0/$app.dll" }
    }
}
finally {
    Exec { docker container rm -f $containerId }
}
