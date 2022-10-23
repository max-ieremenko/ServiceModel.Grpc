@{ 
    Solution      = "InterfaceInheritance.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{ App = "Demo.ServerAspNetCore/bin/Release/net6.0/Demo.ServerAspNetCore.dll" }
        ),
        @( 
            @{ App = "Demo.ServerSelfHost/bin/Release/net6.0/Demo.ServerSelfHost.dll" }
        )
    )
}