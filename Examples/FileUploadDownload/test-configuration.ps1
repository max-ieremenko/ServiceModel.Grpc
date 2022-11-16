@{ 
    Solution      = "FileUploadDownload.sln"
    Configuration = "Debug"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{
                App  = "ServerAspNetHost/bin/Debug/net7.0/ServerAspNetHost.dll"
                Port = 5000
            }
            @{
                App  = "ServerSelfHost/bin/Debug/net7.0/ServerSelfHost.dll"
                Port = 5003
            }
            @{ App = "ConsoleClient/bin/Debug/net7.0/ConsoleClient.dll" }
        ),     
        @( 
            @{ App = "Benchmarks/bin/Debug/net7.0/Benchmarks.dll" }
        )     
    )
}