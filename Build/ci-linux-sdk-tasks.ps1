. (Join-Path $PSScriptRoot "scripts/Import-All.ps1")

Enter-Build {
    $settings = @{
        build    = Get-FullPath $PSScriptRoot;
        sources  = Get-FullPath (Join-Path $PSScriptRoot "../Sources");
        examples = Get-FullPath (Join-Path $PSScriptRoot "../Examples");
        buildOut = Get-FullPath (Join-Path $PSScriptRoot "../build-out");
    }
}

task Default Init, JustBuild, BuildAndRun

task Init {
    exec { dotnet nuget add source $settings.buildOut }
}

task JustBuild {
    $examples = @(
        "CreateClientAndServerASPNETCore"
        , "grpc-dotnet-Compressor"
        , "grpc-dotnet-Counter"
        , "grpc-dotnet-Interceptor"
        , "JsonWebTokenAuthentication"
        , "Swagger\NSwagSwagger.sln"
        , "Swagger\SwashbuckleSwagger.sln")

    foreach ($example in $examples) {
        Invoke-Build -File "sdk-test/just-build.ps1" -Settings $settings -Example $example
    }
}

task BuildAndRun {
    $tests = Get-ChildItem -Path (Join-Path $settings.build sdk-test) -Filter "*-ci-linux.ps1" | ForEach-Object {$_.FullName}
    foreach ($test in $tests) {
        Invoke-Build -File $test -Settings $settings
    }
}