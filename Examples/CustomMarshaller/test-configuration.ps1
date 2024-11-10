@{ 
    Solution      = 'CustomMarshaller.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        @( 
            @{ App = 'Demo.ServerAspNetCore/bin/Release/Demo.ServerAspNetCore.dll' }
        ),
        @( 
            @{ App = 'Demo.ServerSelfHost/bin/Release/Demo.ServerSelfHost.dll' }
        )
    )
}