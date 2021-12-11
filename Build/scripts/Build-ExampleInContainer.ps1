function Build-ExampleInContainer {
    param (
        [Parameter(Mandatory = $true)]
        $Sources,

        [Parameter(Mandatory = $true)]
        $Examples,

        [Parameter(Mandatory = $true)]
        $Packages,

        [Parameter(Mandatory = $true)]
        $ExampleName,

        [Parameter(Mandatory = $true)]
        $DotNet,

        [Parameter()]
        [ValidateSet("Release", "Debug")]
        $Configuration = "Release"
    )

    $imageName = Get-DockerDotNetImageSdk $DotNet

    $containerId = exec {
        docker run -d -it `
            -v "${Sources}:/Sources" `
            -v "${Examples}:/examples" `
            -v "${Packages}:/packages" `
            $imageName
    }
    
    try {
        Write-Output "=== add package source ==="
        exec { docker exec -it $containerId dotnet nuget add source /packages }
    
        $exampleDir = "/examples/$ExampleName";

        Write-Output "=== restore ==="
        exec { docker exec -it $containerId dotnet restore $exampleDir }
    
        Write-Output "=== build ==="
        exec { docker exec -it $containerId dotnet build --configuration $Configuration $exampleDir }
    }
    finally {
        exec { docker container rm -f $containerId }
    }
}