function Get-FullPath {
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        $Path
    )
    
    [System.IO.Path]::GetFullPath($Path)
}