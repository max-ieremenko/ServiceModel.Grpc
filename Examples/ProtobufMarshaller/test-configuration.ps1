@{ 
    Solution      = 'ProtobufMarshaller.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'ServerAspNetCore/bin/Release/ServerAspNetCore.dll'
                Port = 5000
            }
            @{
                App  = 'ServerSelfHost/bin/Release/ServerSelfHost.dll'
                Port = 7000
            }
            @{ App = 'Client/bin/Release/Client.dll' }
            @{ App = 'ClientDesignTime/bin/Release/ClientDesignTime.dll' }
        )
    )
}