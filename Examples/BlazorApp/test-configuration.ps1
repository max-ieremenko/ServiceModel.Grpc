@{ 
    Solution      = "BlazorApp.sln"
    Configuration = "Release"
    Platform      = "Linux"

    Tests         = @(
        , @( 
            @{
                App  = "Server/bin/Release/net6.0/BlazorApp.Server.dll"
                Port = 5000
            }
            @{ App = "Invoke-WebRequest http://localhost:5000/IWeatherForecastService/GetForecasts -Method Post -ContentType application/grpc-web-text" }
        )     
    )
}