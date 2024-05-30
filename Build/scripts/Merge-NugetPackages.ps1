function Merge-NugetPackages {
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Source,

        [Parameter(Mandatory, ValueFromPipeline)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Destination
    )

    if ($ZipAsFolder.ZipExtensions -notcontains '.snupkg') {
        $ZipAsFolder.ZipExtensions += '.snupkg'
    }

    $sourceNuspecFile = Get-Item (Join-Path "zf:$Source" '*.nuspec')
    assert $sourceNuspecFile ".nuspec not found in $Source"

    [xml]$sourceNuspec = Get-Content $sourceNuspecFile
    $ns = New-Object -TypeName 'Xml.XmlNamespaceManager' -ArgumentList $sourceNuspec.NameTable
    $ns.AddNamespace('n', $sourceNuspec.DocumentElement.NamespaceURI)

    $sourceId = $sourceNuspec.SelectSingleNode('n:package/n:metadata/n:id', $ns).InnerText

    $destinationNuspecFile = Get-Item (Join-Path "zf:$Destination" '*.nuspec')
    assert $destinationNuspecFile ".nuspec not found in $Destination"

    [xml]$destinationNuspec = Get-Content $destinationNuspecFile
    foreach ($node in $destinationNuspec.SelectNodes("n:package/n:metadata/n:dependencies/n:group/n:dependency[@id = '$sourceId']", $ns)) {
        $node.ParentNode.RemoveChild($node) | Out-Null
    }

    $tempSpecFile = [System.IO.Path]::GetTempFileName()
    $destinationNuspec.Save($tempSpecFile)
    Set-Content -Path $destinationNuspecFile -Value (Get-Content $tempSpecFile -Raw)
    Remove-Item $tempSpecFile

    $soureName = Split-Path -Path $Source -Leaf
    $destinationName = Split-Path -Path $Destination -Leaf

    $sourceFiles = Get-ChildItem (Join-Path "zf:$Source" 'lib') -Recurse -File
    foreach ($sourceFile in $sourceFiles) {
        $destinationFile = $sourceFile.FullName -replace $soureName, $destinationName
        Copy-Item -Path $sourceFile -Destination $destinationFile
    }

    Remove-Item -Path $Source -Force
}