@{ 
    Solution      = "ClientFilters.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        ,@( 
            @{ App = "Server/bin/Release/net8.0/Server.dll" }
        )
    )
}