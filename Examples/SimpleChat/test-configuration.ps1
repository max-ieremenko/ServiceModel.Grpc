@{ 
    Solution      = "SimpleChat.sln"
    Configuration = "Release"
    Platform      = "Linux"
    BuildMode     = "Publish"

    Tests         = @(
        , @( 
            @{
                App  = "SimpleChat.Server/bin/Release/net8.0/publish/SimpleChat.Server.dll"
                Port = 8080
            }
			@{ App = "SimpleChat.Client.Tester/bin/Release/net8.0/publish/SimpleChat.Client.Tester.dll" }
        )
    )
}