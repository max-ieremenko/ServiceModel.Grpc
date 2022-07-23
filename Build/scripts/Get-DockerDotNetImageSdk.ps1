function Get-DockerDotNetImageSdk {
    param (
        [Parameter(Mandatory = $true)]
        [ValidateSet("netcoreapp3.1", "net6.0")]
        $DotNet
    )
    
    if ($DotNet -eq "netcoreapp3.1") {
        return "mcr.microsoft.com/dotnet/core/sdk:3.1"
    }

    return "mcr.microsoft.com/dotnet/sdk:6.0"
}