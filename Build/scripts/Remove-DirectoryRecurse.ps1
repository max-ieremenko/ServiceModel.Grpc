function Remove-DirectoryRecurse {
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        $Path,

        [Parameter(Position = 1, Mandatory = $false)]
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
