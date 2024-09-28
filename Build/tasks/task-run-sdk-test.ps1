[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]
    $Name,

    [Parameter(Mandatory)]
    [object[]]
    $Apps
)

. (Join-Path $PSScriptRoot '../scripts/Start-Server.ps1')

task . Validate, Run

Exit-Build {
    $jobs = Get-Job -Name 'smgrpc-sdk-*'
    foreach ($job in $jobs) {
        Stop-Job $job
        Receive-Job $job -ErrorAction SilentlyContinue
        Remove-Job $job
    }
}

task Validate {
    foreach ($app in $Apps) {
        if ($app.Type -notin 'exe', 'dll') {
            continue
        }

        $path = $app.App
        if (-not (Test-Path $path)) {
            throw "File $path not found."
        }
    }
}

task Run {
    foreach ($app in $Apps) {
        $path = $app.App
        if ($app.Port) {
            Write-Output "Run job $path"
            Start-Server -JobNamePrefix 'smgrpc-sdk-' -Path $path -WaitTcpPort $app.Port -Type $app.Type
            continue
        }

        if ($app.Type -eq 'dll') {
            Write-Output "Run dotnet $path"
            exec { dotnet $path }
        }
        elseif ($app.Type -eq 'exe') {
            Write-Output "Run $path"
            exec { & $path }
        }
        else {
            Write-Output "Invoke $path"
            Invoke-Expression $path
        }
    }
}