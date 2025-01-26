@{ 
    Solution      = 'MessagePackMarshaller.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'Server/bin/Release/Server.dll'
                Port = 8080
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )     
    )
}