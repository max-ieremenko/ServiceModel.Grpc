@{ 
    Solution      = "NSwagSwagger.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "NSwagWebApplication/bin/Release/net8.0/NSwagWebApplication.dll"
                Port = 5001
            }
            @{ App = "Client/bin/Release/net8.0/Client.dll" }
        )     
    )
}