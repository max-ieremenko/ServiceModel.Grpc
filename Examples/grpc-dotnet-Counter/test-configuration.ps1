@{ 
    Solution      = "Counter.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "Server/bin/Release/net8.0/Server.dll"
                Port = 5000
            }
            @{ App = "Client/bin/Release/net8.0/Client.dll" }
        )     
    )
}