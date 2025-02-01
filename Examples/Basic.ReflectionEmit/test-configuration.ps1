@{ 
    Solution      = 'Basic.ReflectionEmit.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'Server/bin/Release/Server.dll'
                Port = 8081
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )     
    )
}