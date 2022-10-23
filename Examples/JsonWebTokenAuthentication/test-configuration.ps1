@{ 
    Solution      = "JsonWebTokenAuthentication.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "WebApplication/bin/Release/net6.0/WebApplication.dll"
                Port = 8080
            }
            @{ App = "Client/bin/Release/net6.0/Client.dll" }
        )
    )
}