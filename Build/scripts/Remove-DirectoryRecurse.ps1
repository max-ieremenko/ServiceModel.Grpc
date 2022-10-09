function Remove-DirectoryRecurse {
    param (
        [Parameter(Mandatory)]
        [string]
        $Path,

        [Parameter()]
        [string[]]
        $Filters
    )

    if (-not (Test-Path $Path)) {
        return
    }

    if ($Filters) {
        foreach ($filter in $Filters) {
            Get-ChildItem -Path $Path -Filter $filter -Directory -Recurse | Remove-Item -Recurse -Force
        }
    }
    else {
        Remove-Item -Path $Path -Recurse -Force
    }
}
