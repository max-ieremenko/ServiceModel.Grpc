function Get-ReleaseVersion {
    param (
        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $Sources
    )

    $versionsProps = Join-Path $Sources 'Versions.props'
    (Select-Xml -Path $versionsProps -XPath 'Project/PropertyGroup/ServiceModelGrpcVersion').Node.InnerText
}