@{ 
    Solution      = "SimpleChat.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "SimpleChat.Server/bin/Release/net8.0/SimpleChat.Server.dll"
                Port = 8080
            }
			@{ App = "SimpleChat.Client.Tester/bin/Release/net8.0/SimpleChat.Client.Tester.dll" }
        )
    )
}