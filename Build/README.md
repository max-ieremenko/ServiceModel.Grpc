# Build

## To run local build

Because of the dependency on net461, the build runs on Windows.

- install dependencies

[net6.0 sdk](https://dotnet.microsoft.com/download/dotnet/6.0), 
[InvokeBuild](https://www.powershellgallery.com/packages/InvokeBuild/5.9.12), 
[ThirdPartyLibraries](https://www.powershellgallery.com/packages/ThirdPartyLibraries/3.1.2)
[ZipAsFolder](https://www.powershellgallery.com/packages/ZipAsFolder/0.0.1)

    ``` powershell
    PS> ./Build/install-dependencies.ps1
    ```

- switch docker to linux containers

- run build

    ``` powershell
    PS> ./Build/build-locally.ps1
    ```
