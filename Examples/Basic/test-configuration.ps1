@{ 
    Solution      = "Basic.sln"
    Configuration = "Release"
    Platform      = "Win"

    Tests         = @(
        @( 
            @{ App = "Demo.AspNet.DesignTime/bin/Release/net6.0/Demo.AspNet.DesignTime.dll" }
        ),
        @( 
            @{ App = "Demo.AspNet.DesignTime/bin/Release/net6.0/Demo.AspNet.DesignTime.dll" }
        ),     
        @( 
            @{ App = "Demo.AspNet.ReflectionEmit/bin/Release/net6.0/Demo.AspNet.ReflectionEmit.dll" }
        ),     
        @( 
            @{ App = "Demo.SelfHost.DesignTime/bin/Release/net6.0/Demo.SelfHost.DesignTime.dll" }
        ),     
        @( 
            @{ App = "Demo.SelfHost.ReflectionEmit/bin/Release/net6.0/Demo.SelfHost.ReflectionEmit.dll" }
        ),     
        @( 
            @{ App = "Demo.SelfHost.DesignTime/bin/Release/net462/Demo.SelfHost.DesignTime.exe" }
        ),     
        @( 
            @{ App = "Demo.SelfHost.ReflectionEmit/bin/Release/net462/Demo.SelfHost.ReflectionEmit.exe" }
        )     
    )
}