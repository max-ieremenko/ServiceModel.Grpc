@{ 
    Solution      = "CreateClientAndServerASPNETCore.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "Service/bin/Release/net8.0/Service.dll"
                Port = 5000
            }
            @{ App = "Client/bin/Release/net8.0/Client.dll" }
            @{ App = "ClientDesignTime/bin/Release/net8.0/ClientDesignTime.dll" }
        )     
    )
}