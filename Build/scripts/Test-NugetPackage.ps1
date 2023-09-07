function Test-NugetPackage {
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Path
    )

    function Test-NuGetSpec {
        param (
            [string]
            $Path
        )
        
        [xml]$nuspec = Get-Content $Path
        $ns = New-Object -TypeName "Xml.XmlNamespaceManager" -ArgumentList $nuspec.NameTable
        $ns.AddNamespace("n", $nuspec.DocumentElement.NamespaceURI)
    
        $name = $nuspec.SelectSingleNode("n:package/n:metadata/n:id", $ns).InnerText
    
        $repository = $nuspec.SelectSingleNode("n:package/n:metadata/n:repository", $ns)
        assert $repository "Repository element not found in $name"
    
        $commit = $nuspec.SelectSingleNode("n:package/n:metadata/n:repository/@commit", $ns)
        assert ($commit -and $commit.Value) "Repository commit attribute not found in $name"
    }

    $name = Split-Path $Path -Leaf

    assert (Test-Path (Join-Path "zf:$Path" "LICENSE")) "LICENSE file not found in name"
    assert (Test-Path (Join-Path "zf:$Path" "ThirdPartyNotices.txt")) "ThirdPartyNotices.txt file not found in name"
    assert (Test-Path (Join-Path "zf:$Path" "README.md")) "README.md file not found in name"

    # test .nuspec
    $nuspecFile = Get-Item (Join-Path "zf:$Path" "*.nuspec")
    assert $nuspecFile ".nuspec not found in $name"
    Test-NuGetSpec $nuspecFile.FullName

    $symbolFileName = [System.IO.Path]::ChangeExtension($Path, ".snupkg")
    assert (Test-Path $symbolFileName) "$symbolFileName not found"
}
