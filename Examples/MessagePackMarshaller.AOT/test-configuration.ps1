@{ 
    Solution      = 'MessagePackMarshaller.AOT.sln'
    Configuration = 'Release'
    Platform      = 'Linux'
    BuildMode     = 'Publish'

    Tests         = @(
        , @( 
            @{
                App  = 'Server/bin/Release/linux-x64/publish/Server'
                Type = 'exe'
                Port = 5000
            }
            @{ 
                App  = 'Client/bin/Release/linux-x64/publish/Client'
                Type = 'exe'
            }
        )     
    )
}