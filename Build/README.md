# Build

## To run local build

Because of the dependency on net462, the build runs on Windows.

- install dependencies

[net9.0 sdk](https://dotnet.microsoft.com/download/dotnet/9.0), 
[InvokeBuild](https://www.powershellgallery.com/packages/InvokeBuild/5.11.3),
[ThirdPartyLibraries](https://www.powershellgallery.com/packages/ThirdPartyLibraries/3.6.0),
[ZipAsFolder](https://www.powershellgallery.com/packages/ZipAsFolder/1.0.0)

    ``` powershell
    PS> ./Build/install-dependencies.ps1
    ```

- switch docker to linux containers

- run build

    ``` powershell
    PS> ./Build/build-locally.ps1
    ```
