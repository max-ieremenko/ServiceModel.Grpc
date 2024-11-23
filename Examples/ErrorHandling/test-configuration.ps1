@{ 
    Solution      = 'ErrorHandling.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'ServerAspNetHost/bin/Release/ServerAspNetHost.dll'
                Port = 5000
            }
            @{
                App  = 'ServerNativeHost/bin/Release/ServerNativeHost.dll'
                Port = 5050
            }
            @{ App = 'Client/bin/Release/Client.dll' }
            @{ App = 'ClientDesignTime/bin/Release/ClientDesignTime.dll' }
        )     
    )
}