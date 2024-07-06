[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]
    $Name,

    [Parameter(Mandatory)]
    [object[]]
    $Apps
)

. (Join-Path $PSScriptRoot "../scripts/Start-Server.ps1")

task . Run

Exit-Build {
    foreach ($app in $script:runningServers) {
        $app.Kill()
    }
}

task Run {
    $script:runningServers = @()

    foreach ($app in $Apps) {
        $path = $app.App
        if ($app.Port) {
            $script:runningServers += Start-Server -Path $path -WaitTcpPort $app.Port
            continue
        }

        if ($path.EndsWith(".dll", "OrdinalIgnoreCase")) {
            exec { dotnet $path }
        }
        elseif ($path.EndsWith(".exe", "OrdinalIgnoreCase")) {
            exec { & $path }
        }
        else {
            Invoke-Expression $path
        }
    }
}