@{ 
    Solution      = "ProtobufMarshaller.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "ServerAspNetCore/bin/Release/net8.0/ServerAspNetCore.dll"
                Port = 5000
            }
            @{
                App  = "ServerSelfHost/bin/Release/net8.0/ServerSelfHost.dll"
                Port = 7000
            }
            @{ App = "Client/bin/Release/net8.0/Client.dll" }
            @{ App = "ClientDesignTime/bin/Release/net8.0/ClientDesignTime.dll" }
        )
    )
}