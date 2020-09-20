Task default -Depends Clean, Build, ThirdPartyNotices, UnitTest, Pack, PackTest
Task UnitTest -Depends UnitTest461, UnitTestCore21, UnitTestCore31

Task Clean {
    $binDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\build-out"))
    if (Test-Path $binDir) {
        Remove-Item -Path $binDir -Recurse -Force
    }
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