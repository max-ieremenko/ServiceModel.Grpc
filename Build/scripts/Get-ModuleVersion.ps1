function Get-ModuleVersion {
    param (
        [Parameter(Mandatory)]
        [string]
        $Name
    )
    
    $sources = Get-Content (Join-Path $PSScriptRoot "../invoke-ci-build.ps1") -Raw
    $tokens = $null
    $errors = $null
    $modules = [Management.Automation.Language.Parser]::ParseInput($sources, [ref]$tokens, [ref]$errors).ScriptRequirements.RequiredModules
    foreach ($module in $modules) {
        if ($module.Name -eq $Name) {
            return $module.Version
        }
    }

    throw "Module $Name no found."
}
