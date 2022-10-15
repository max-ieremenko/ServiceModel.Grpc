param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathBuildArtifacts,

    [Parameter(Mandatory)]
    [hashtable[]]
    $Solutions
)

task Default Clean, BuildParallel, BuildSequential, Run

Enter-Build {
    Clear-NugetCache
    exec { dotnet nuget add source -n "ServiceModel.Grpc" $PathBuildArtifacts }
}

Exit-Build {
    exec { dotnet nuget remove source "ServiceModel.Grpc" }
    Clear-NugetCache
}

task Clean {
    foreach ($solution in $solutions) {
        $path = Split-Path $solution.Path -Parent
        Remove-DirectoryRecurse -Path $path -Filters "bin", "obj"
    }
}

task BuildParallel {
    $builds = @()
    foreach ($solution in $solutions) {
        if ($solution.BuildParallelizable) {
            $builds += @{ 
                File          = "task-build.ps1";
                Path          = $solution.Path;
                Configuration = $solution.Configuration
            }
        }
    }

    Build-Parallel $builds -ShowParameter Path -MaximumBuilds 4
}

task BuildSequential {
    $builds = @()
    foreach ($solution in $solutions) {
        if (-not $solution.BuildParallelizable) {
            $builds += @{ 
                File          = "task-build.ps1";
                Path          = $solution.Path;
                Configuration = $solution.Configuration
            }
        }
    }

    Build-Parallel $builds -ShowParameter Path -MaximumBuilds 1
}

task Run {
    $builds = @()
    foreach ($solution in $solutions) {
        $root = Split-Path $solution.Path -Parent
        
        foreach ($app in $solution.Apps) {
            $path = Join-Path $root $app "bin" $solution.Configuration $solution.Framework "$app.dll"
            $builds += @{ 
                File = "task-dotnet-run.ps1";
                Path = $path;
            }
        }
    }

    # only one app at a time: ports conflict
    Build-Parallel $builds -ShowParameter Path -MaximumBuilds 1
}
