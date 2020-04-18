Include ".\build-scripts.ps1"

Task default -Depends Initialize, Clean, Build, ThirdPartyNotices, Pack
Task ThirdPartyNotices -Depends ThirdPartyCore, ThirdPartyAspNetCore, ThirdPartySelfHost, ThirdPartyProtoBuf
Task Pack -Depends PackCore, PackAspNetCore, PackSelfHost, PackProtoBuf

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

Task ThirdPartyCore {
    $appNames = @("Core")
    $sources = @(
        (Join-Path $script:sourceDir "ServiceModel.Grpc"),
        (Join-Path $script:sourceDir "ServiceModel.Grpc.Test"),
        (Join-Path $script:sourceDir "ServiceModel.Grpc.TestApi")
    )

    Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir
}

Task ThirdPartyAspNetCore {
    $appNames = @("AspNetCore", "Core")
    $sources = @(
        (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore"),
        (Join-Path $script:sourceDir "ServiceModel.Grpc.AspNetCore.Test")
    )

    Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir
}

Task ThirdPartySelfHost {
    $appNames = @("SelfHost", "Core")
    $sources = @(
        (Join-Path $script:sourceDir "ServiceModel.Grpc.SelfHost"),
        (Join-Path $script:sourceDir "ServiceModel.Grpc.SelfHost.Test")
    )

    Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir
}

Task ThirdPartyProtoBuf {
    $appNames = @("ProtoBuf", "Core")
    $sources = @(
        (Join-Path $script:sourceDir "ServiceModel.Grpc.ProtoBufMarshaller")
    )

    Write-ThirdPartyNotices $appNames $sources $thirdPartyRepository $binDir
}

Task PackCore {
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

Task PackAspNetCore {
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

Task PackSelfHost {
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

Task PackProtoBuf {
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
