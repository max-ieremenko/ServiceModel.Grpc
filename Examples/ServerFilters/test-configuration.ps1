@{ 
    Solution      = "ServerFilters.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{ App = "ServerAspNetHost/bin/Release/net6.0/ServerAspNetHost.dll" }
        ),
        @( 
            @{ App = "ServerSelfHost/bin/Release/net6.0/ServerSelfHost.dll" }
        )
    )
}