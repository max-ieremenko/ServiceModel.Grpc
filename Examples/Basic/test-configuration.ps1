@{ 
    Solution      = 'Basic.sln'
    Configuration = 'Release'
    Platform      = 'Win'

    Tests         = @(
        @( 
            @{ App = 'Demo.AspNet.DesignTime/bin/Release/Demo.AspNet.DesignTime.exe' }
        ),
        @( 
            @{ App = 'Demo.AspNet.ReflectionEmit/bin/Release/Demo.AspNet.ReflectionEmit.exe' }
        )
    )
}