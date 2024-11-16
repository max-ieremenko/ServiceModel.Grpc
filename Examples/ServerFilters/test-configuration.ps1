@{ 
    Solution      = 'ServerFilters.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        @( 
            @{ App = 'ServerAspNetHost/bin/Release/ServerAspNetHost.dll' }
        ),
        @( 
            @{ App = 'ServerSelfHost/bin/Release/ServerSelfHost.dll' }
        )
    )
}