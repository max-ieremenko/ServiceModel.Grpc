@{ 
    Solution      = "ProtobufMarshaller.sln"
    Configuration = "Release"
    Platform      = "Win"

    Tests         = @(
        , @( 
            @{
                App  = "ServerAspNetCore/bin/Release/netcoreapp3.1/ServerAspNetCore.exe"
                Port = 5000
            }
            @{
                App  = "ServerSelfHost/bin/Release/net462/ServerSelfHost.exe"
                Port = 7000
            }
            @{ App = "Client/bin/Release/net462/Client.exe" }
            @{ App = "ClientDesignTime/bin/Release/net462/ClientDesignTime.exe" }
        )
    )
}