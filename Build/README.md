# Build

The repository is configured to run [CI build](https://github.com/max-ieremenko/ServiceModel.Grpc/actions) on any push or pull request into the master/release branch.

## Local build

To run CI build locally

- install [InvokeBuild](https://www.powershellgallery.com/packages/InvokeBuild)

    ``` powershell
    PS> Install-Module -Name InvokeBuild -RequiredVersion 5.9.9.0
    ```

- install net6.0 sdk: manual [download](https://dotnet.microsoft.com/download/dotnet/6.0) or

    ``` powershell
    PS> .\Build\step-install-dotnet.ps1
    ```

- install ThirdPartyLibraries

    ``` powershell
    PS> Install-Module -Name ThirdPartyLibraries -RequiredVersion 3.1.2
    ```

- switch docker to linux containers

- run build

    ``` powershell
    PS> .\Build\build-locally.ps1
    ```
