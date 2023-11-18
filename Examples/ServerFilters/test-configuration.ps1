@{ 
    Solution      = "ServerFilters.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{ App = "ServerAspNetHost/bin/Release/net8.0/ServerAspNetHost.dll" }
        ),
        @( 
            @{ App = "ServerSelfHost/bin/Release/net8.0/ServerSelfHost.dll" }
        )
    )
}