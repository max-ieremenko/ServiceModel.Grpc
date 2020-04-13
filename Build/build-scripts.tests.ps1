#Install-Module -Name Pester
#Requires -Modules @{ModuleName='Pester'; RequiredVersion='4.10.1';}

. .\build-scripts.ps1

Describe "Get-PackageVersion" {
    $testCases = @{ version = "1.0.0"; expected = "1.0.0" } `
        ,@{ version = "1.0.0-pre1"; expected = "1.0.0-pre1" }

    It "Extract version from <version>" -TestCases $testCases {
        param ($version, $expected)

        $content = "[assembly: AssemblyInformationalVersion(""" + $version + """)]"

        Mock -CommandName Get-Content `
            -MockWith { return @($content) } `
            -ParameterFilter { $Path -eq "AssemblyInfo.cs" }
        
        $actual = Get-PackageVersion "AssemblyInfo.cs"
        
        $actual | Should -Be $expected
    }
}