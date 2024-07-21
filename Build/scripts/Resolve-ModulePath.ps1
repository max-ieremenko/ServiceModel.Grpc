function Resolve-ModulePath {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)]
        [string]
        $Name,
    
        [Parameter(Mandatory)]
        [string]
        $Version
    )
    
    $test = Get-InstalledModule -Name $Name -MinimumVersion $Version -ErrorAction 'SilentlyContinue'
    if (-not $test) {
        throw "Module $Name $Version not found."
    }
    
    $test.InstalledLocation
}
