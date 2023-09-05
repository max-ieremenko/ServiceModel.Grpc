[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Sources,

    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $BuildOut
)

task Default Core, AspNetCore, Swashbuckle, NSwag, DesignTime, SelfHost, ProtoBufMarshaller, MessagePackMarshaller, Test

task Core {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc\ServiceModel.Grpc.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task AspNetCore {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.AspNetCore\ServiceModel.Grpc.AspNetCore.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task Swashbuckle {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.AspNetCore.Swashbuckle\ServiceModel.Grpc.AspNetCore.Swashbuckle.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task NSwag {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.AspNetCore.NSwag\ServiceModel.Grpc.AspNetCore.NSwag.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task DesignTime {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.DesignTime\ServiceModel.Grpc.DesignTime.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task SelfHost {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.SelfHost\ServiceModel.Grpc.SelfHost.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task ProtoBufMarshaller {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.ProtoBufMarshaller\ServiceModel.Grpc.ProtoBufMarshaller.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task MessagePackMarshaller {
    $projectFile = Join-Path $Sources "ServiceModel.Grpc.MessagePackMarshaller\ServiceModel.Grpc.MessagePackMarshaller.csproj"
    exec {
        dotnet pack `
            -c Release `
            --no-build `
            --property:NoWarn=NU5104 `
            -o $BuildOut `
            $projectFile
    }
}

task Test {
    $packageList = Get-ChildItem -Path $BuildOut -Recurse -Filter *.nupkg | ForEach-Object { $_.FullName }
    assert ($packageList.Count) "no packages found"
    
    $packageList | Test-NugetPackage
}