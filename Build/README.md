# Build

The repository is configured to run [CI build](https://github.com/max-ieremenko/test/actions) on any push or pull request into the master branch.

## Local build

To run CI build locally

- install [psake](https://www.powershellgallery.com/packages/psake)

    ``` powershell
    PS> Install-Module -Name psake
    ```

- install [net6.0 sdk](https://dotnet.microsoft.com/download/dotnet/6.0)

- install ThirdPartyLibraries

    ``` powershell
    PS> dotnet tool install --global ThirdPartyLibraries.GlobalTool
    ```

- switch docker to linux containers

- run build

    ``` powershell
    PS> .\Build\build-locally.ps1
    ```
