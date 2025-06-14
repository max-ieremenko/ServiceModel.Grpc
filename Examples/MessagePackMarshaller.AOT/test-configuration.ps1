@{ 
    Solution      = 'MessagePackMarshaller.AOT.slnx'
    Configuration = 'Release'
    Platform      = 'Linux', 'MacOS'
    BuildMode     = 'Publish'

    Tests         = @(
        , @( 
            @{
                App  = 'Server/bin/Release/publish/Server'
                Type = 'exe'
                Port = 5000
            }
            @{ 
                App  = 'Client/bin/Release/publish/Client'
                Type = 'exe'
            }
        )     
    )
}