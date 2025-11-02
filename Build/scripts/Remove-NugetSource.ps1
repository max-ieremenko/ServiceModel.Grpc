function Remove-NugetSource {
    param (
        [Parameter()]
        [string]
        $Name = 'ServiceModel.Grpc.Build'
    )
    
    $found = $false

    $sources = exec { dotnet nuget list source --format Detailed }
    foreach ($source in $sources) {
        if ($source.Contains(" $Name ", 'OrdinalIgnoreCase')) {
            $found = $true
            break
        }
    }

    if ($found) {
        exec { dotnet nuget remove source $Name }
    }
}