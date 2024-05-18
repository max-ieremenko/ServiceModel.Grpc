param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $PathBuildArtifacts,

    [Parameter(Mandatory)]
    [object[]]
    $Examples
)

task Default Clean, BuildParallel, BuildSequential, Run

Enter-Build {
    Clear-NugetCache

    $ownSource = $true
    $sources = exec { dotnet nuget list source --format short }
    foreach ($source in $sources) {
        if ($source.Contains($PathBuildArtifacts, "OrdinalIgnoreCase")) {
            $ownSource = $false
            break
        }
    }

    if ($ownSource) {
        exec { dotnet nuget add source -n "ServiceModel.Grpc" $PathBuildArtifacts }
    }
}

Exit-Build {
    if ($ownSource) {
        exec { dotnet nuget remove source "ServiceModel.Grpc" }
    }

    Clear-NugetCache
}

task Clean {
    foreach ($example in $Examples) {
        $path = Split-Path $example.Solution -Parent
        Remove-DirectoryRecurse -Path $path -Filters "bin", "obj"
    }
}

task BuildParallel {
    $builds = @()
    foreach ($example in $Examples) {
        if ($example.BuildParallelizable) {
            $builds += @{ 
                File          = "task-build.ps1"
                Path          = $example.Solution
                Configuration = $example.Configuration
                Mode          = $example.BuildMode
            }
        }
    }

    Build-Parallel $builds -ShowParameter Path -MaximumBuilds 4
}

task BuildSequential {
    $builds = @()
    foreach ($example in $Examples) {
        if (-not $example.BuildParallelizable) {
            $builds += @{ 
                File          = "task-build.ps1"
                Path          = $example.Solution
                Configuration = $example.Configuration
            }
        }
    }

    Build-Parallel $builds -ShowParameter Path -MaximumBuilds 1
}

task Run {
    $builds = @()
    foreach ($example in $Examples) {

        foreach ($test in $example.Tests) {
            $builds += @{ 
                File = "task-run-sdk-test.ps1"
                Name = $test[0].App
                Apps = $test
            }   
        }
    }

    # only one app at a time: ports conflict
    Build-Parallel $builds -ShowParameter Name -MaximumBuilds 1
}
