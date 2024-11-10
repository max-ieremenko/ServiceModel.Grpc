@{ 
    Solution      = 'FileUploadDownload.sln'
    Configuration = 'Debug'
    Platform      = 'Linux'

    Tests         = @(
        @( 
            @{
                App  = 'ServerAspNetHost/bin/Debug/ServerAspNetHost.dll'
                Port = 5000
            }
            @{
                App  = 'ServerSelfHost/bin/Debug/ServerSelfHost.dll'
                Port = 5003
            }
            @{ App = 'ConsoleClient/bin/Debug/ConsoleClient.dll' }
        ),     
        @( 
            @{ App = 'Benchmarks/bin/Debug/Benchmarks.dll' }
        )     
    )
}