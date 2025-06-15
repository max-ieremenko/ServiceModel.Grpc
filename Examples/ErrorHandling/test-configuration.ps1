@{ 
    Solution      = 'ErrorHandling.slnx'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'Server/bin/Release/Server.dll'
                Port = 5000
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )     
    )
}