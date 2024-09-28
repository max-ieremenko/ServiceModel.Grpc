function Start-Server {
    param (
        [Parameter(Mandatory)]
        [string]
        $JobNamePrefix,

        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Path,
        
        [Parameter(Mandatory)]
        [int]
        $WaitTcpPort,

        [Parameter(Mandatory)]
        [ValidateSet('exe', 'dll')]
        [string]
        $Type
    )
    
    $name = Split-Path $Path -Leaf
    $jobName = "$JobNamePrefix$([Guid]::NewGuid())"

    if ($Type -eq 'exe') {
        $job = Start-Job -Name $jobName -ScriptBlock { & $args } -ArgumentList $Path
    }
    else {
        $job = Start-Job -Name $jobName -ScriptBlock { dotnet $args } -ArgumentList $Path
    }

    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    for ($i = 0; $i -lt 10; $i++) {
        Start-Sleep -Seconds 1
        $test = Test-Connection -TargetName localhost -TcpPort $WaitTcpPort
        if ($test) {
            return
        }

        $job = Get-Job -Name $jobName
        if ($job.State -ne 'Running') {
            throw "$name exited unexpectedly, state is $($process.State)."
        }
    }

    throw "$name port $WaitTcpPort is not available during $($timer.Elapsed)."
}