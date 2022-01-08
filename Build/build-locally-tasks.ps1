. (Join-Path $PSScriptRoot "scripts/Import-All.ps1")

Enter-Build {
    $settings = @{
        build      = Get-FullPath $PSScriptRoot;
        sources    = Get-FullPath (Join-Path $PSScriptRoot "..\Sources");
        examples   = Get-FullPath (Join-Path $PSScriptRoot "..\Examples");
        benchmarks = Get-FullPath (Join-Path $PSScriptRoot "..\Benchmarks");
        buildOut   = Get-FullPath (Join-Path $PSScriptRoot "..\build-out");
        thirdParty = Get-FullPath (Join-Path $PSScriptRoot "third-party-libraries");
    }
}

task Default Clean, Init, Build, ThirdPartyNotices, UnitTest, Pack, PackTest, SdkTest, Benchmarks
task UnitTest UnitTest461, UnitTestCore31, UnitTestNet50, UnitTestNet60

task Clean {
    Remove-DirectoryRecurse -Path $settings.buildOut

    Remove-DirectoryRecurse -Path $settings.sources -Filters "bin", "obj"
    Remove-DirectoryRecurse -Path $settings.examples -Filters "bin", "obj"
    Remove-DirectoryRecurse -Path $settings.benchmarks -Filters "bin", "obj"

    Get-ChildItem -Path (Join-Path $env:USERPROFILE ".nuget\packages") -Filter "servicemodel.grpc*" -Directory | Remove-Item -Force -Recurse
}

task Init {
    $env:GITHUB_SHA = exec { git rev-parse HEAD }
}

task Build {
    Invoke-Build -File "step-build.ps1" -Settings $settings
}

task UnitTest461 {
    Invoke-Build -File "step-unit-test.ps1" -Settings $settings -Framework net461
}

task UnitTestCore31 {
    Invoke-Build -File "step-unit-test.ps1" -Settings $settings -Framework netcoreapp3.1
}

task UnitTestNet50 {
    Invoke-Build -File "step-unit-test.ps1" -Settings $settings -Framework net5.0
}

task UnitTestNet60 {
    Invoke-Build -File "step-unit-test.ps1" -Settings $settings -Framework net6.0
}

task ThirdPartyNotices {
    Invoke-Build -File "step-third-party-notices.ps1" -Settings $settings
}

task Pack {
    Invoke-Build -File "step-pack.ps1" -Settings $settings
}

task PackTest {
    Invoke-Build -File "step-pack-test.ps1" -Settings $settings
}

task SdkTest {
    $tests = Get-ChildItem -Path (Join-Path $settings.build sdk-test) -Filter "*-locally.ps1" | ForEach-Object {$_.FullName}
    foreach ($test in $tests) {
        Invoke-Build -File $test -Settings $settings
    }
}

task Benchmarks {
    Invoke-Build -File "step-benchmarks-locally.ps1" -Settings $settings
}