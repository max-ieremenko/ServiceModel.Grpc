Include ".\build-scripts.ps1"

Task default -Depends Initialize, Clean, Build, CreateThirdPartyNotices, Pack
Task Pack -Depends PackServiceModelGrpc, PackServiceModelGrpcAspNetCore, PackServiceModelGrpcSelfHost, PackServiceModelGrpcProtoBufMarshaller

Task Initialize {
    $script:sourceDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))
    $script:binDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
    $script:thirdPartyRepository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\ServiceModel.Grpc.ThirdPartyLibraries"))
    $script:packageVersion = Get-PackageVersion (Join-Path $sourceDir "GlobalAssemblyInfo.cs")
    $script:repositoryCommitId = Get-RepositoryCommitId
    
    Write-Host "PackageVersion: $packageVersion"
    Write-Host "CommitId: $repositoryCommitId"
}

Task Clean {
    if (Test-Path $binDir) {
        Remove-Item -Path $binDir -Recurse -Force
    }
}

Task Build {
    $solutionFile = Join-Path $sourceDir "ServiceModel.Grpc.sln"
    Exec { dotnet restore $solutionFile }
    Exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
}

Task CreateThirdPartyNotices {
    Exec {
        ThirdPartyLibraries update `
            -appName ServiceModel.Grpc `
            -source $sourceDir `
            -repository $thirdPartyRepository
    }
  
    Exec {
        ThirdPartyLibraries validate `
            -appName ServiceModel.Grpc `
            -source $sourceDir `
            -repository $thirdPartyRepository
    }

    Exec {
        ThirdPartyLibraries generate `
            -appName ServiceModel.Grpc `
            -repository $thirdPartyRepository `
            -to $binDir
    }

    $licenseFiles = Join-Path $binDir "Licenses"
    if (Test-Path $licenseFiles) {
        Remove-Item -Path $licenseFiles -Recurse -Force
    }
}

Task PackServiceModelGrpc {
    $projectFile = Join-Path $sourceDir "ServiceModel.Grpc\ServiceModel.Grpc.csproj"

    Exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:PackageVersion=$packageVersion `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $binDir `
            $projectFile
    }
}

Task PackServiceModelGrpcAspNetCore {
    $projectFile = Join-Path $sourceDir "ServiceModel.Grpc.AspNetCore\ServiceModel.Grpc.AspNetCore.csproj"

    Exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:PackageVersion=$packageVersion `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $binDir `
            $projectFile
    }
}

Task PackServiceModelGrpcSelfHost {
    $projectFile = Join-Path $sourceDir "ServiceModel.Grpc.SelfHost\ServiceModel.Grpc.SelfHost.csproj"

    Exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:PackageVersion=$packageVersion `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $binDir `
            $projectFile
    }
}

Task PackServiceModelGrpcProtoBufMarshaller {
    $projectFile = Join-Path $sourceDir "ServiceModel.Grpc.ProtoBufMarshaller\ServiceModel.Grpc.ProtoBufMarshaller.csproj"

    Exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:PackageVersion=$packageVersion `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $binDir `
            $projectFile
    }
}
