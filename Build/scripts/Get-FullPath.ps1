function Get-FullPath {
    param (
        [Parameter(Mandatory)]
        [string]
        $Path
    )
    
    [System.IO.Path]::GetFullPath($Path)
}