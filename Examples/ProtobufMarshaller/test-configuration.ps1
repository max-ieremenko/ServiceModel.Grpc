@{ 
    Solution      = "ProtobufMarshaller.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "ServerAspNetCore/bin/Release/net7.0/ServerAspNetCore.dll"
                Port = 5000
            }
            @{
                App  = "ServerSelfHost/bin/Release/net7.0/ServerSelfHost.dll"
                Port = 7000
            }
            @{ App = "Client/bin/Release/net7.0/Client.dll" }
            @{ App = "ClientDesignTime/bin/Release/net7.0/ClientDesignTime.dll" }
        )
    )
}