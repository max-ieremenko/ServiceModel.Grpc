[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    $Settings
)

Enter-Build {
    $repositoryCommitId = $env:GITHUB_SHA    
}

task Default Core, AspNetCore, Swashbuckle, NSwag, DesignTime, SelfHost, ProtoBufMarshaller, MessagePackMarshaller

task Core {
    # ServiceModel.Grpc
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc\ServiceModel.Grpc.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task AspNetCore {
    # ServiceModel.Grpc.AspNetCore
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore\ServiceModel.Grpc.AspNetCore.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task Swashbuckle {
    # ServiceModel.Grpc.AspNetCore.Swashbuckle
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.Swashbuckle\ServiceModel.Grpc.AspNetCore.Swashbuckle.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task NSwag {
    # ServiceModel.Grpc.AspNetCore.NSwag
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.AspNetCore.NSwag\ServiceModel.Grpc.AspNetCore.NSwag.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task DesignTime {
    # ServiceModel.Grpc.DesignTime
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.DesignTime\ServiceModel.Grpc.DesignTime.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task SelfHost {
    # ServiceModel.Grpc.SelfHost
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.SelfHost\ServiceModel.Grpc.SelfHost.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task ProtoBufMarshaller {
    # ServiceModel.Grpc.ProtoBufMarshaller
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.ProtoBufMarshaller\ServiceModel.Grpc.ProtoBufMarshaller.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}

task MessagePackMarshaller {
    # ServiceModel.Grpc.MessagePackMarshaller
    $projectFile = Join-Path $Settings.sources "ServiceModel.Grpc.MessagePackMarshaller\ServiceModel.Grpc.MessagePackMarshaller.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $Settings.buildOut `
            $projectFile
    }
}