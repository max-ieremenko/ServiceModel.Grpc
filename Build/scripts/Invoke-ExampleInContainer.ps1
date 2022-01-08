function Invoke-ExampleInContainer {
    param (
        [Parameter(Mandatory = $true)]
        $Example,

        [Parameter(Mandatory = $true)]
        $DotNet,

        [Parameter()]
        [ValidateSet("Release", "Debug")]
        $Configuration = "Release",

        [Parameter(Mandatory = $true)]
        [string[]]
        $Apps
    )
 
    $imageName = Get-DockerDotNetImageSdk $DotNet

    $containerId = exec {
        docker run -d -it -v "${Example}:/example" $imageName
    }

    try {
        foreach ($app in $Apps) {
            Write-Output "=== exec $app ==="

            $path = "/example/$app/bin/$Configuration/$DotNet/$app.dll"
            Exec { docker exec -it $containerId dotnet $path }
        }
    }
    finally {
        Exec { docker container rm -f $containerId }
    }
}