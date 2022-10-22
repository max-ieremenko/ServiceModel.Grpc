@{ 
    Solution      = "ErrorHandling.sln"
    Configuration = "Release"
    Platform      = "Win"

    Tests         = @(
        , @( 
            @{
                App  = "ServerAspNetHost/bin/Release/net6.0/ServerAspNetHost.exe"
                Port = 5000
            }
            @{
                App  = "ServerNativeHost/bin/Release/net462/ServerNativeHost.exe"
                Port = 5050
            }
            @{ App = "Client/bin/Release/net462/Client.exe" }
            @{ App = "ClientDesignTime/bin/Release/net462/ClientDesignTime.exe" }
        )     
    )
}