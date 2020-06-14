function Get-PackageVersion($assemblyInfoCsPath) {
    $Anchor = "AssemblyInformationalVersion(""";
    $lines = Get-Content -Path $assemblyInfoCsPath

    foreach ($line in $lines) {
        $index = $line.IndexOf($Anchor);
        if ($index -lt 0) {
            continue;
        }
        
        $text = $line.Substring($index + $Anchor.Length);

        $index = $text.IndexOf('"');
        $text = $text.Substring(0, $index);
    
        return $text;
    }
}
