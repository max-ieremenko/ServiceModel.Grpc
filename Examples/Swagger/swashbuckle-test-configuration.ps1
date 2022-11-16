@{ 
    Solution      = "SwashbuckleSwagger.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "SwashbuckleWebApplication/bin/Release/net7.0/SwashbuckleWebApplication.dll"
                Port = 5001
            }
            @{ App = "Client/bin/Release/net7.0/Client.dll" }
        )     
    )
}