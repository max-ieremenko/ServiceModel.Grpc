@{ 
    Solution      = "ClientFilters.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        ,@( 
            @{ App = "Server/bin/Release/net7.0/Server.dll" }
        )
    )
}