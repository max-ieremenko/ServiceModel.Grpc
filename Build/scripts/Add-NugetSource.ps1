function Add-NugetSource {
    param (
        [Parameter(Mandatory)]
        [string]
        $Path,

        [Parameter()]
        [string]
        $Name = 'ServiceModel.Grpc.Build'
    )
    
    $sources = exec { dotnet nuget list source --format Short }
    foreach ($source in $sources) {
        if ($source.Contains($Path, 'OrdinalIgnoreCase')) {
            return
        }
    }

    exec { dotnet nuget add source --name $Name $Path }
}