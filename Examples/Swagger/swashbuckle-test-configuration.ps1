@{ 
    Solution      = 'SwashbuckleSwagger.slnx'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'SwashbuckleWebApplication/bin/Release/SwashbuckleWebApplication.dll'
                Port = 5001
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )     
    )
}