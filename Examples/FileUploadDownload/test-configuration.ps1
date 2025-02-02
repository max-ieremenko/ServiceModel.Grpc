@{ 
    Solution      = 'FileUploadDownload.sln'
    Configuration = 'Debug'
    Platform      = 'Linux'

    Tests         = @(
        @( 
            @{
                App  = 'Server/bin/Debug/Server.dll'
                Port = 5000
            }
            @{ App = 'Client/bin/Debug/Client.dll' }
        ),     
        @( 
            @{ App = 'Benchmarks/bin/Debug/Benchmarks.dll' }
        )     
    )
}