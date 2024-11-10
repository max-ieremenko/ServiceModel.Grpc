@{ 
    Solution      = 'JsonWebTokenAuthentication.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'WebApplication/bin/Release/WebApplication.dll'
                Port = 8080
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )
    )
}