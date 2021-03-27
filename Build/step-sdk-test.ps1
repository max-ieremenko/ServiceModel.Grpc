$packageDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
$examplesDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Examples"))

Get-ChildItem -Recurse -Path (Join-Path $examplesDir "obj") -Directory | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Path (Join-Path $examplesDir "bin") -Directory | Remove-Item -Recurse -Force

$containerId = Exec {
    docker run -d -it -v "${examplesDir}:/examples" -v "${packageDir}:/packages" mcr.microsoft.com/dotnet/sdk:5.0-alpine
}

try {
    Write-Host "=== add package source ==="
    Exec { docker exec -it $containerId dotnet nuget add source /packages }

    Write-Host "=== restore ==="
    Exec { docker exec -it $containerId dotnet restore /examples/Basic }
    if (-not $?) {
        throw "restore faulted."
    }

    Write-Host "=== build ==="
    Exec { docker exec -it $containerId dotnet build --configuration Release /examples/Basic }
    if (-not $?) {
        throw "build faulted."
    }
}
finally {
    Exec { docker container rm -f $containerId }
}

$containerId = Exec {
    docker run -d -it -v "${examplesDir}:/examples" mcr.microsoft.com/dotnet/core/sdk:3.1
}

try {
    $apps = @("Demo.AspNet.DesignTime", "Demo.AspNet.ReflectionEmit", "Demo.SelfHost.DesignTime", "Demo.SelfHost.ReflectionEmit")
    foreach ($app in $apps) {
        Write-Host "=== exec $app ==="
        Exec { docker exec -it $containerId dotnet "/examples/Basic/$app/bin/Release/netcoreapp3.1/$app.dll" }
        if (-not $?) {
            throw "$app does not work."
        }
    }
}
finally {
    Exec { docker container rm -f $containerId }
}
