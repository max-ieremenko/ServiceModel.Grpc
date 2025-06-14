@{ 
    Solution      = 'SimpleChat.slnx'
    Configuration = 'Release'
    Platform      = 'Linux'
    BuildMode     = 'Publish'

    Tests         = @(
        , @( 
            @{
                App  = 'SimpleChat.Server/bin/Release/publish/SimpleChat.Server.dll'
                Port = 8080
            }
            @{ App = 'SimpleChat.Client.Tester/bin/Release/publish/SimpleChat.Client.Tester.dll' }
        )
    )
}