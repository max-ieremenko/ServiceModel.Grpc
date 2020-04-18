function Get-PackageVersion($assemblyInfoCsPath) {
    $Anchor = "AssemblyInformationalVersion(""";
    $lines = Get-Content -Path $assemblyInfoCsPath

    foreach ($line in $lines) {
        $index = $line.IndexOf($Anchor);
        if ($index -lt 0) {
            continue;
        }
        
        $text = $line.Substring($index + $Anchor.Length);

        $index = $text.IndexOf('"');
        $text = $text.Substring(0, $index);
    
        return $text;
    }
}

function Get-RepositoryCommitId {
    $response = (Invoke-RestMethod -Uri "https://api.github.com/repos/max-ieremenko/ServiceModel.Grpc/commits/master")
    return $response.sha
}

function Write-ThirdPartyNotices($appNames, $sources, $repository, $out) {
    $appName = $appNames[0]
    $generateAppNames = $appNames | ForEach-Object {"-appName", $_}
    $source = $sources | ForEach-Object {"-source", $_}
    $outTemp = Join-Path $out "Temp"

    Exec {
        ThirdPartyLibraries update `
            -appName $appName `
            $source `
            -repository $repository
    }
  
    Exec {
        ThirdPartyLibraries validate `
            -appName $appName `
            $source `
            -repository $repository
    }

    Exec {
        ThirdPartyLibraries generate `
            $generateAppNames `
            -repository $repository `
            -to $outTemp
    }

    $licenseFile = $appName + "ThirdPartyNotices.txt"
    Move-Item (Join-Path $outTemp "ThirdPartyNotices.txt") (Join-Path $out $licenseFile) -Force
    Remove-Item -Path $outTemp -Recurse -Force
}