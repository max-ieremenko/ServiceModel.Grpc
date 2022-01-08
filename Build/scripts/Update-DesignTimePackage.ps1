function FunctionName {
    param (
        $binDir
    )
    
    function Get-Version($target, $ns, $id) {
        $node = $target.SelectSingleNode("s:dependency[@id = '" + $id + "']", $ns)
        if ($node) {
            return $node.GetAttribute("version")
        }
    
        throw "Dependency " + $id + "not found."
    }
    
    function Add-Dependency($target, $id, $version) {
        $doc = $target.OwnerDocument
        $dependency = $doc.CreateElement("dependency", $doc.DocumentElement.NamespaceURI)
        
        $idAttribute = $doc.CreateAttribute("id")
        $idAttribute.InnerText = $id
    
        $versionAttribute = $doc.CreateAttribute("version")
        $versionAttribute.InnerText = $version
    
        $includeAttribute = $doc.CreateAttribute("include")
        $includeAttribute.InnerText = "All"
    
        $dependency.Attributes.Append($idAttribute) | Out-Null
        $dependency.Attributes.Append($versionAttribute) | Out-Null
        $dependency.Attributes.Append($includeAttribute) | Out-Null
    
        $target.AppendChild($dependency) | Out-Null
    }
        
    $packageFile = Get-ChildItem -Path $binDir -Filter ServiceModel.Grpc.DesignTime.?.?.*.nupkg | ForEach-Object {$_.FullName}
    if ($packageFile.Count -ne 1) {
        throw "ServiceModel.Grpc.DesignTime.*.nupkg not found."
    }

    $tempPath = Join-Path ([System.IO.Path]::GetTempPath()) "step-pack"
    Remove-DirectoryRecurse $tempPath

    New-Item -Path $tempPath -ItemType Directory | Out-Null

    $tempPackageFileName = Join-Path $tempPath "temp.zip"
    Copy-Item -Path $packageFile -Destination $tempPackageFileName
    Expand-Archive -Path $tempPackageFileName -DestinationPath $tempPath
    Remove-Item $tempPackageFileName

    $specFileName = Join-Path $tempPath "ServiceModel.Grpc.DesignTime.nuspec";
    [xml]$spec = Get-Content $specFileName
    $ns = New-Object -TypeName "Xml.XmlNamespaceManager" -ArgumentList $spec.NameTable
    $ns.AddNamespace("s", $spec.DocumentElement.NamespaceURI)
    $dependencies = $spec.SelectNodes("s:package/s:metadata/s:dependencies/s:group", $ns)

    if ($dependencies.Count -ne 1) {
        throw "ServiceModel.Grpc.DesignTime.nuspec does not contain any dependencies."
    }

    $serviceModelVersion = $spec.SelectNodes("s:package/s:metadata/s:version", $ns).InnerText
    foreach ($dependency in $dependencies) {
        Get-Version $dependency $ns "ServiceModel.Grpc" | Out-Null # assert
        $roslynVersion = Get-Version $dependency $ns "CodeGeneration.Roslyn.Attributes"

        Add-Dependency $dependency "CodeGeneration.Roslyn.Tool" $roslynVersion
        Add-Dependency $dependency "ServiceModel.Grpc.DesignTime.Generator" $serviceModelVersion
    }

    $spec.Save($specFileName)

    Compress-Archive -Path (Join-Path $tempPath "*")  -DestinationPath ($packageFile + ".zip")
    Remove-Item $packageFile
    Rename-Item ($packageFile + ".zip") $packageFile

    Remove-Item -Path $tempPath -Force -Recurse
}
