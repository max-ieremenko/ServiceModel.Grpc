@{ 
    Solution      = 'Basic.sln'
    Configuration = 'Release'
    Platform      = 'Win'

    Tests         = @(
        @( 
            @{ App = 'Demo.AspNet.DesignTime/bin/Release/Demo.AspNet.DesignTime.exe' }
        ),
        @( 
            @{ App = 'Demo.AspNet.DesignTime/bin/Release/Demo.AspNet.DesignTime.exe' }
        ),     
        @( 
            @{ App = 'Demo.AspNet.ReflectionEmit/bin/Release/Demo.AspNet.ReflectionEmit.exe' }
        ),     
        @( 
            @{ App = 'Demo.SelfHost.DesignTime/bin/Release/net9.0/Demo.SelfHost.DesignTime.exe' }
        ),     
        @( 
            @{ App = 'Demo.SelfHost.ReflectionEmit/bin/Release/net9.0/Demo.SelfHost.ReflectionEmit.exe' }
        ),     
        @( 
            @{ App = 'Demo.SelfHost.DesignTime/bin/Release/net462/Demo.SelfHost.DesignTime.exe' }
        ),     
        @( 
            @{ App = 'Demo.SelfHost.ReflectionEmit/bin/Release/net462/Demo.SelfHost.ReflectionEmit.exe' }
        )     
    )
}