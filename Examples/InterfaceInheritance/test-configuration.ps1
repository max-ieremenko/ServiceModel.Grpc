@{ 
    Solution      = "InterfaceInheritance.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        @( 
            @{ App = "Demo.ServerAspNetCore/bin/Release/net7.0/Demo.ServerAspNetCore.dll" }
        ),
        @( 
            @{ App = "Demo.ServerSelfHost/bin/Release/net7.0/Demo.ServerSelfHost.dll" }
        )
    )
}