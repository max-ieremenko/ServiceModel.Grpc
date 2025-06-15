@{ 
    Solution      = 'CompatibilityWithNativegRPC.slnx'
    Configuration = 'Release'
    Platform      = 'Linux'

    Tests         = @(
        , @( 
            @{
                App  = 'Server.CodeFirst/bin/Release/Server.CodeFirst.dll'
                Port = 5000
            }
            @{
                App  = 'Server.Proto/bin/Release/Server.Proto.dll'
                Port = 5001
            }
            @{ App = 'Client/bin/Release/Client.dll' }
        )     
    )
}