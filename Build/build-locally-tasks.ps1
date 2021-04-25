Task default -Depends Clean, Init, Build, ThirdPartyNotices, UnitTest, Pack, PackTest, SdkTest
Task UnitTest -Depends UnitTest461, UnitTestCore21, UnitTestCore31, UnitTestNet50
Task SdkTest -Depends SdkTestBasic, SdkTestMessagePack

Task Clean {
    $binDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
    if (Test-Path $binDir) {
        Remove-Item -Path $binDir -Recurse -Force
    }
}

Task Init {
    $env:GITHUB_SHA = Exec { git rev-parse HEAD }
}

Task Build {
    Exec { .\step-build.ps1 }
}

Task UnitTest461 {
    Exec { .\step-unit-test.ps1 -Framework net461 }
}

Task UnitTestCore21 {
    Exec { .\step-unit-test.ps1 -Framework netcoreapp2.1 }
}

Task UnitTestCore31 {
    Exec { .\step-unit-test.ps1 -Framework netcoreapp3.1 }
}

Task UnitTestNet50 {
    Exec { .\step-unit-test.ps1 -Framework net5.0 }
}

Task ThirdPartyNotices {
    Exec { .\step-third-party-notices.ps1 }
}

Task Pack {
    Exec { .\step-pack.ps1 }
}

Task PackTest {
    Exec { .\step-pack-test.ps1 }
}

Task SdkTestBasic {
    Exec { .\step-sdk-test-basic-locally.ps1 }
}

Task SdkTestMessagePack {
    Exec { .\step-sdk-test-messagepack-locally.ps1 }
}