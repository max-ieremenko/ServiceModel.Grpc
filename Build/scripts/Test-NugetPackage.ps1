function Test-NugetPackage {
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $PackageFileName,

        [Parameter(Mandatory = $true)]
        [string]
        $TempDirectory
    )

    function Test-FileExists {
        param (
            [string]$packageName,
            [string]$directory,
            [string]$fileName
        )
    
        $path = Join-Path $directory $fileName
        if (-not (Test-Path $path)) {
            throw ("File " + $fileName + " not found in " + $name)
        }
    }
    
    function Test-NuGetSpec {
        param (
            [string]$fileName
        )
        
        [xml]$nuspec = Get-Content $fileName
        $ns = New-Object -TypeName "Xml.XmlNamespaceManager" -ArgumentList $nuspec.NameTable
        $ns.AddNamespace("n", $nuspec.DocumentElement.NamespaceURI)
    
        $name = $nuspec.SelectNodes("n:package/n:metadata/n:id", $ns).InnerText
    
        $repository = $nuspec.SelectNodes("n:package/n:metadata/n:repository", $ns)
        if (-not $repository) {
            throw ("Repository element not found in " + $name)
        }
    
        if ([string]::IsNullOrWhiteSpace($nuspec.SelectNodes("n:package/n:metadata/n:repository/@commit", $ns).Value)) {
            throw ("Repository commit attribute not found in " + $name)
        }
    }

    Remove-DirectoryRecurse $tempPath
    New-Item -Path $TempDirectory -ItemType Directory | Out-Null

    # un-zip package into TempDirectory
    $tempPackageFileName = Join-Path $TempDirectory "test.zip"
    Copy-Item -Path $PackageFileName -Destination $tempPackageFileName
    Expand-Archive -Path $tempPackageFileName -DestinationPath $TempDirectory
    Remove-Item $tempPackageFileName

    $name = Split-Path $PackageFileName -Leaf

    Test-FileExists $name $TempDirectory "LICENSE"
    Test-FileExists $name $TempDirectory "ThirdPartyNotices.txt"

    # test .nuspec
    $nuspecFile = Get-ChildItem -Path $TempDirectory -File -Filter *.nuspec | ForEach-Object {$_.FullName}

    if (-not $nuspecFile) {
        throw (".nuspec not found in " + $name)
    }

    Test-NuGetSpec $nuspecFile

    $symbolFileName = [System.IO.Path]::ChangeExtension($PackageFileName, ".snupkg")
    if (-not (Test-Path $symbolFileName)) {
        throw ("Symbol package " + $symbolFileName + " not found")
    }
}
