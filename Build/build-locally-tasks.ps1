Task default -Depends Clean, Init, Build, ThirdPartyNotices, UnitTest, Pack, PackTest, SdkTest, Benchmarks
Task UnitTest -Depends UnitTest461, UnitTestCore31, UnitTestNet50, UnitTestNet60
Task SdkTest -Depends SdkTestBasic, SdkTestMessagePack

Task Clean {
    $dir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
    if (Test-Path $dir) {
        Remove-Item -Path $dir -Recurse -Force
    }

    $dir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))
    Get-ChildItem -Path $dir -Filter bin -Directory -Recurse | Remove-Item -Recurse -Force
    Get-ChildItem -Path $dir -Filter obj -Directory -Recurse | Remove-Item -Recurse -Force

    $dir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Examples"))
    Get-ChildItem -Path $dir -Filter bin -Directory -Recurse | Remove-Item -Recurse -Force
    Get-ChildItem -Path $dir -Filter obj -Directory -Recurse | Remove-Item -Recurse -Force

    $dir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Benchmarks"))
    Get-ChildItem -Path $dir -Filter bin -Directory -Recurse | Remove-Item -Recurse -Force
    Get-ChildItem -Path $dir -Filter obj -Directory -Recurse | Remove-Item -Recurse -Force
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

Task UnitTestCore31 {
    Exec { .\step-unit-test.ps1 -Framework netcoreapp3.1 }
}

Task UnitTestNet50 {
    Exec { .\step-unit-test.ps1 -Framework net5.0 }
}

Task UnitTestNet60 {
    Exec { .\step-unit-test.ps1 -Framework net6.0 }
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

Task Benchmarks {
    Exec { .\step-benchmarks-locally.ps1 }
}