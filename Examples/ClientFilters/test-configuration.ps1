@{ 
    Solution      = 'ClientFilters.sln'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{ App = 'Server/bin/Release/Server.dll' }
        )
    )
}