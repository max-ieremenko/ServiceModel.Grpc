@{ 
    Solution      = "BlazorApp.sln"
    Configuration = "Release"
    Platform      = "Linux"
    BuildMode     = "Publish"

    Tests         = @(
        , @( 
            @{
                App  = "Server/bin/Release/net8.0/publish/BlazorApp.Server.dll"
                Port = 5000
            }
            @{ App = "Invoke-WebRequest http://localhost:5000/IWeatherForecastService/GetForecasts -Method Post -ContentType application/grpc-web-text" }
			@{ App = "ConsoleClient/bin/Release/net8.0/publish/ConsoleApp.Client.dll" }
        )     
    )
}