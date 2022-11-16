@{ 
    Solution      = "CreateClientAndServerASPNETCore.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "Service/bin/Release/net7.0/Service.dll"
                Port = 5000
            }
            @{ App = "Client/bin/Release/net7.0/Client.dll" }
            @{ App = "ClientDesignTime/bin/Release/net7.0/ClientDesignTime.dll" }
        )     
    )
}