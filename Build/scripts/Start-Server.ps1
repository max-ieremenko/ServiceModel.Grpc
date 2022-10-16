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

    if ($name.EndsWith(".exe", "OrdinalIgnoreCase")) {
        $process = Start-Process `
            -FilePath $Path `
            -PassThru `
            -NoNewWindow `
            -RedirectStandardOutput $output
    }
    else {
        $process = Start-Process `
            -FilePath dotnet `
            -PassThru `
            -NoNewWindow `
            -ArgumentList $Path `
            -RedirectStandardOutput $output
    }

    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    for ($i = 0; $i -lt 10; $i++) {
        Start-Sleep -Seconds 1
        $test = Test-Connection -TargetName localhost -TcpPort $WaitTcpPort
        if ($test) {
            return $process
        }

        $process.Refresh()
        if ($process.HasExited) {
            throw "$Name exited unexpectedly $($timer.Elapsed)"        
        }
    }

    $process.Kill()
    throw "$Name port $WaitTcpPort is not available during $($timer.Elapsed)"
}