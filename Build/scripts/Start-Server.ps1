function Start-Server {
    param (
        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Path,
        
        [Parameter(Mandatory)]
        [int]
        $WaitTcpPort
    )
    
    $name = Split-Path $Path -Leaf
    $output = Join-Path ([System.IO.Path]::GetTempPath()) "smgrpc-sdk-$name.txt"
    if (Test-Path $output) {
        Remove-Item $output -Force
    }

    $workingDirectory = Split-Path -Path $Path -Parent
    if ($name.EndsWith('.exe', 'OrdinalIgnoreCase')) {
        $process = Start-Process `
            -FilePath $Path `
            -PassThru `
            -NoNewWindow `
            -WorkingDirectory $workingDirectory `
            -RedirectStandardOutput $output
    }
    else {
        $process = Start-Process `
            -FilePath dotnet `
            -PassThru `
            -NoNewWindow `
            -WorkingDirectory $workingDirectory `
            -ArgumentList $Path `
            -RedirectStandardOutput $output
    }

    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    $logs = ''
    for ($i = 0; $i -lt 10; $i++) {
        Start-Sleep -Seconds 1
        $test = Test-Connection -TargetName localhost -TcpPort $WaitTcpPort
        if ($test) {
            return $process
        }

        $process.Refresh()
        if ($process.HasExited) {
            if (Test-Path $output) {
                $logs = Get-Content -Path $output -Raw
            }

            throw "$Name exited unexpectedly with exit code $($process.ExitCode), $($timer.Elapsed). $logs"        
        }
    }

    $process.Kill()
    if (Test-Path $output) {
        $logs = Get-Content -Path $output -Raw
    }

    throw "$Name port $WaitTcpPort is not available during $($timer.Elapsed). $logs"
}