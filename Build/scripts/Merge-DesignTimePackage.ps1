function Merge-DesignTimePackage {
    param (
        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ -PathType Container })]
        [string]
        $Sources,

        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ -PathType Leaf })]
        [string]
        $PackagePath
    )
 
    function Get-NugetPackagePath {
        param (
            [Parameter(Mandatory)]
            [string]
            $Name
        )

        $packageProps = Join-Path $Sources 'Directory.Packages.props'
        $version = (Select-Xml -Path $packageProps -XPath "Project/ItemGroup/PackageVersion[@Include = '$Name']").Node.Attributes['Version'].Value

        Join-Path (Get-NugetPath) "$($Name.ToLowerInvariant())" $version 'lib/netstandard2.0'
    }

    $tempSource = Join-Path $env:TEMP ([Guid]::NewGuid())
    $tempOutput = Join-Path $env:TEMP ([Guid]::NewGuid())
    New-Item -Path $tempOutput -ItemType Directory | Out-Null

    try {
        Expand-Archive -Path $PackagePath -DestinationPath $tempSource

        $primary = Get-Item -Path (Join-Path $tempSource 'analyzers/dotnet/roslyn4.0/cs/*.dll')
        $other = Get-ChildItem -Path (Join-Path $tempSource 'build/dependencies') -Filter '*.dll'
        $dest = Join-Path $tempOutput $primary.Name
        $keyfile = Join-Path $Sources 'ServiceModel.Grpc.snk'

        $codeAnalysis = Get-NugetPackagePath -Name 'Microsoft.CodeAnalysis.Common'
        $bcl = Get-NugetPackagePath -Name 'Microsoft.Bcl.AsyncInterfaces'

        exec {
            dotnet ilrepack `
                /out:$dest `
                /lib:$codeAnalysis `
                /lib:$bcl `
                /keyfile:$keyfile `
                $primary `
                $other
        }

        Remove-Item (Join-Path $tempSource 'build/dependencies') -Force -Recurse
        Move-Item $dest $primary -Force
        Remove-Item $PackagePath

        Compress-Archive "$tempSource/*" $PackagePath
    }
    finally {
        Remove-Item -Path $tempSource -Force -Recurse
        Remove-Item -Path $tempOutput -Force -Recurse
    }
}