@{ 
    Solution      = "ErrorHandling.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "ServerAspNetHost/bin/Release/net7.0/ServerAspNetHost.dll"
                Port = 5000
            }
            @{
                App  = "ServerNativeHost/bin/Release/net7.0/ServerNativeHost.dll"
                Port = 5050
            }
            @{ App = "Client/bin/Release/net7.0/Client.dll" }
            @{ App = "ClientDesignTime/bin/Release/net7.0/ClientDesignTime.dll" }
        )     
    )
}