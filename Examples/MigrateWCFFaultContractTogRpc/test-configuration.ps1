@{ 
    Solution      = "MigrateWCFFaultContractTogRpc.sln"
    Configuration = "Release"
    Platform      = "Win"

    Tests         = @(
        @( 
            @{
                App  = "WCFServiceHost/bin/Release/net462/WCFServiceHost.exe"
                Port = 8000
            }
            @{ App = "WCFClient/bin/Release/net462/WCFClient.exe" }
        ),
        @( 
            @{
                App  = "AspNetServiceHost/bin/Release/net6.0/AspNetServiceHost.exe"
                Port = 8080
            }
            @{
                App  = "NativeServiceHost/bin/Release/net462/NativeServiceHost.exe"
                Port = 8090
            }
            @{ App = "gRPCClient/bin/Release/net462/gRPCClient.exe" }
            @{ App = "gRPCClientDesignTime/bin/Release/net462/gRPCClientDesignTime.exe" }
        )
    )
}