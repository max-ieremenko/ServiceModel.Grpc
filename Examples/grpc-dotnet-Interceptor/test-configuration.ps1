@{ 
    Solution      = 'Interceptor.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        @( 
            @{
                App  = 'ServerAspNetHost/bin/Release/ServerAspNetHost.dll'
                Port = 5000
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        ),
        @( 
            @{
                App  = 'ServerSelfHost/bin/Release/ServerSelfHost.dll'
                Port = 5000
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )
    )
}