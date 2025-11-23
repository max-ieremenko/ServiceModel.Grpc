# Build

## To run local build

Because of the dependency on net462, the build runs on Windows.

- install dependencies

[net10.0 sdk](https://dotnet.microsoft.com/download/dotnet/10.0), 
[InvokeBuild](https://www.powershellgallery.com/packages/InvokeBuild),
[ThirdPartyLibraries](https://www.powershellgallery.com/packages/ThirdPartyLibraries),
[ZipAsFolder](https://www.powershellgallery.com/packages/ZipAsFolder)

    ``` powershell
    PS> ./Build/install-dependencies.ps1
    ```

- switch docker to linux containers

- run build

    ``` powershell
    PS> ./Build/build-locally.ps1
    ```
