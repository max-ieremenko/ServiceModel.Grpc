. (Join-Path $PSScriptRoot "scripts/Import-All.ps1")

Enter-Build {
    $settings = @{
        sources    = Get-FullPath (Join-Path $PSScriptRoot "..\Sources");
        buildOut   = Get-FullPath (Join-Path $PSScriptRoot "..\build-out");
        thirdParty = Get-FullPath (Join-Path $PSScriptRoot "third-party-libraries");
    }
}

task Default Build, ThirdPartyNotices, UnitTest, Pack, PackTest
task UnitTest UnitTest461, UnitTestCore31, UnitTestNet50, UnitTestNet60

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