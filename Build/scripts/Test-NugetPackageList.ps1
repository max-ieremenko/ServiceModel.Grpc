function Test-NugetPackageList {
    param (
        [Parameter(Mandatory)]
        [string[]]
        $Name,

        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ -PathType Container })]
        [string]
        $BuildOut
    )

    $found = New-Object -TypeName System.Collections.Generic.HashSet[string]
    foreach ($file in (Get-ChildItem -Path $BuildOut -Filter '*.*nupkg' -File)) {
        $found.Add($file.Name) | Out-Null
    }

    foreach ($expected in $Name) {
        assert $found.Remove("$expected.nupkg") "$expected.nupkg not found"
        assert $found.Remove("$expected.snupkg") "$expected.snupkg not found"
    }

    assert (-not $found.Count) "found unxpected $($found.Count) NuGets"
}