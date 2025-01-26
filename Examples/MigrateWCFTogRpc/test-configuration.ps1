@{ 
    Solution      = 'MigrateWCFTogRpc.sln'
    Configuration = 'Release'
    Platform      = 'Win'

    Tests         = @(
        @( 
            @{
                App  = 'WcfServer/bin/Release/WcfServer.exe'
                Port = 8000
            }
            @{ App = 'WcfClient/bin/Release/WcfClient.exe' }
        ),
        @( 
            @{
                App  = 'GrpcServer/bin/Release/GrpcServer.exe'
                Port = 5000
            }
            @{ App = 'GrpcClient/bin/Release/GrpcClient.exe' }
        )
    )
}