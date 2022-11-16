@{ 
    Solution      = "ServerFilters.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{ App = "ServerAspNetHost/bin/Release/net7.0/ServerAspNetHost.dll" }
        ),
        @( 
            @{ App = "ServerSelfHost/bin/Release/net7.0/ServerSelfHost.dll" }
        )
    )
}