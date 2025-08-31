@{ 
    Solution      = 'NerdbankMessagePackMarshaller.Grpc.Core.slnx'
    Configuration = 'Release'
    Platform      = 'Win'

    Tests         = @(
        , @( 
            @{
                App  = 'Server/bin/Release/Server.exe'
                Port = 8082
            }
            @{ 
                App  = 'Client/bin/Release/Client.exe'
            }
        )     
    )
}