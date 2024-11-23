@{ 
    Solution      = 'CreateClientAndServerASPNETCore.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'Service/bin/Release/Service.dll'
                Port = 5000
            }
            @{ App = 'Client/bin/Release/Client.dll' }
            @{ App = 'ClientDesignTime/bin/Release/ClientDesignTime.dll' }
        )     
    )
}