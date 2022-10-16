@{ 
    Solution      = "Interceptor.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{
                App  = "ServerAspNetHost/bin/Release/net6.0/ServerAspNetHost.dll"
                Port = 5000
            }
            @{ App = "Client/bin/Release/net6.0/Client.dll" }
        ),
        @( 
            @{
                App  = "ServerSelfHost/bin/Release/net6.0/ServerSelfHost.dll"
                Port = 5000
            }
            @{ App = "Client/bin/Release/net6.0/Client.dll" }
        )
    )
}