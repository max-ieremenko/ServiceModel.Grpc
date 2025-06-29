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
 
    $tempSource = Join-Path $env:TEMP ([Guid]::NewGuid())
    $tempOutput = Join-Path $env:TEMP ([Guid]::NewGuid())
    New-Item -Path $tempOutput -ItemType Directory | Out-Null

    try {
        Expand-Archive -Path $PackagePath -DestinationPath $tempSource

        $primary = Get-Item -Path (Join-Path $tempSource 'analyzers/dotnet/roslyn4.0/cs/*.dll')
        $other = Get-ChildItem -Path (Join-Path $tempSource 'build/dependencies') -Filter '*.dll'
        $dest = Join-Path $tempOutput $primary.Name
        $keyfile = Join-Path $Sources 'ServiceModel.Grpc.snk'

        $codeAnalysis = Join-Path (Get-NugetPath) 'microsoft.codeanalysis.common/4.0.1/lib/netstandard2.0'
        $bcl = Join-Path (Get-NugetPath) 'microsoft.bcl.asyncinterfaces/1.0.0/lib/netstandard2.0'

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