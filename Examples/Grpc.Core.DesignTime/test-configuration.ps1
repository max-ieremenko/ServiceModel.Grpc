@{ 
    Solution      = 'Grpc.Core.DesignTime.sln'
    Configuration = 'Release'
    Platform      = 'Win'

    Tests         = @(
        @( 
            @{
                App  = 'Server/bin/Release/net462/Server.exe'
                Port = 8082
            }
            @{ App = 'Client/bin/Release/net462/Client.exe' }
        ),
        @( 
            @{
                App  = 'Server/bin/Release/net9.0/Server.exe'
                Port = 8082
            }
            @{ App = 'Client/bin/Release/net9.0/Client.exe' }
        )
    )
}