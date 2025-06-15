@{ 
    Solution      = 'Basic.DesignTime.slnx'
    Configuration = 'Release'
    Platform      = 'Linux', 'MacOS'

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