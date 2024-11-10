@{ 
    Solution      = 'MigrateWCFTogRpc.sln'
    Configuration = 'Release'
    Platform      = 'Win'

    Tests         = @(
        @( 
            @{
                App  = 'WCFServiceHost/bin/Release/WCFServiceHost.exe'
                Port = 8000
            }
            @{ App = 'WCFClient/bin/Release/WCFClient.exe' }
        ),
        @( 
            @{
                App  = 'AspNetServiceHost/bin/Release/AspNetServiceHost.exe'
                Port = 8080
            }
            @{
                App  = 'NativeServiceHost/bin/Release/NativeServiceHost.exe'
                Port = 8090
            }
            @{ App = 'gRPCClient/bin/Release/gRPCClient.exe' }
            @{ App = 'gRPCClientDesignTime/bin/Release/gRPCClientDesignTime.exe' }
        )
    )
}