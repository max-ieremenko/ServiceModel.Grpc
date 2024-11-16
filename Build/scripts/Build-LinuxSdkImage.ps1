function Build-LinuxSdkImage {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string]
        $Tag = 'service-model-grpc/sdk:9.0-noble',

        [Parameter()]
        [switch]
        $Force
    )
    
    $ErrorActionPreference = 'Stop'
    Set-StrictMode -Version Latest

    $image = docker image ls $Tag --format '{{.Repository}}:{{.Tag}}'
    if ($LASTEXITCODE) {
        throw 'docker image ls failed'
    }

    if ($image -and (-not $Force)) {
        return $Tag
    }

    $dockerfile = Join-Path $PSScriptRoot 'Build-LinuxSdkImage.dockerfile'
    $context = $PSScriptRoot

    docker build --pull -f $dockerfile -t $Tag $context
    if ($LASTEXITCODE) {
        throw 'docker build failed'
    }

    $Tag
}