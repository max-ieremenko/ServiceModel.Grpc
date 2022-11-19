@{ 
    Solution      = "Compressor.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "Server/bin/Release/net7.0/Server.dll"
                Port = 5000
            }
            @{ App = "Client/bin/Release/net7.0/Client.dll" }
        )     
    )
}