@{ 
    Solution      = "CustomMarshaller.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{ App = "Demo.ServerAspNetCore/bin/Release/net8.0/Demo.ServerAspNetCore.dll" }
        ),
        @( 
            @{ App = "Demo.ServerSelfHost/bin/Release/net8.0/Demo.ServerSelfHost.dll" }
        )
    )
}