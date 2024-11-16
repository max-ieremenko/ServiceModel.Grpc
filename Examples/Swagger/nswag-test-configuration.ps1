@{ 
    Solution      = 'NSwagSwagger.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'NSwagWebApplication/bin/Release/NSwagWebApplication.dll'
                Port = 5001
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )     
    )
}