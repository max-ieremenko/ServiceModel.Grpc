@{ 
    Solution      = "CompatibilityWithNativegRPC.sln"
    Configuration = "Release"
    Platform      = "Win"

    Tests         = @(
        , @( 
            @{
                App  = "ServerAspNetHost/bin/Release/netcoreapp3.1/ServerAspNetHost.dll"
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