
$script:BaseUrl = 'http://localhost:64952'

function Get-OctoKitMsbuildLogBaseUrl{
    [CmdletBinding()]
    [OutputType([String])]
    param()
    $script:BaseUrl
}

<#
.SYNOPSIS
Sets the base url

.DESCRIPTION

.PARAMETER Url
Parameter description

.PARAMETER Passthru
Setting this causes $Url to be returned.

.EXAMPLE
Set-OctoKitMsbuildLogBaseUrl http://msbloc.localtest.me:64952
Set-OctoKitMsbuildLogBaseUrl http://localhost:64952

.NOTES
General notes
#>
function Set-OctoKitMsbuildLogBaseUrl{
    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [ValidateNotNullOrEmpty()]
        [string] $Url,
        [switch] $Passthru
    )
    $script:BaseUrl = $Url
    if($Passthru) {
        return $script:BaseUrl
    }
}

function  Send-OctoKitMsbuildLog {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [ValidateScript({ [System.IO.File]::Exists($_)})]
        [ValidateNotNullOrEmpty()]
        [string] $Path,
        # TODO: Add ValidateScript to ensure its a real URI
        [ValidateNotNullOrEmpty()]
        [string] $BaseUri = $script:BaseUrl, #TODO: Replace with production Url
        [ValidateNotNullOrEmpty()]
        [string] $RepoName = $env:APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME,
        [ValidateNotNullOrEmpty()]
        [string] $Branch = $env:APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH,
        [ValidateNotNullOrEmpty()]
        [Alias('Sha', 'CommitHash')]
        [string] $HeadCommit = $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT
    )
    #TODO: Stream this
    $FileBytes = [System.IO.File]::ReadAllBytes($Path);
    $FileEnc = [System.Text.Encoding]::GetEncoding('UTF-8').GetString($FileBytes);
    $FileInfo = New-Object System.IO.FileInfo @($Path)
    $Boundary = [System.Guid]::NewGuid().ToString();
    $LF = "`r`n";
    $Body = @(
        "--$Boundary",
        "Content-Disposition: form-data; name=`"MsbuildLog`"; filename=`"$($FileInfo.Name)`"",
        "Content-Type: application/octet-stream$LF",
        $FileEnc,
        "--$Boundary",
        "Content-Disposition: form-data; name=`"RepoName`"$LF",
        $RepoName
        "--$Boundary",
        "Content-Disposition: form-data; name=`"Branch`"$LF",
        $Branch
        "--$Boundary",
        "Content-Disposition: form-data; name=`"SHA`"$LF",
        $HeadCommit
        "--$Boundary--$LF"
    ) -join $LF

    $Uri = GetUploadUrl $BaseUri
    Invoke-RestMethod `
        -Method POST `
        -Uri $Uri `
        -ContentType "multipart/form-data; boundary=`"$Boundary`"" `
        -Body $Body

}

# Export only the functions using PowerShell standard verb-noun naming.
# Be sure to list each exported functions in the FunctionsToExport field of the module manifest file.
# This improves performance of command discovery in PowerShell.
Export-ModuleMember -Function '*-*'

# "Private" methods
function GetUploadUrl() {
    [CmdletBinding()]
    [OutputType([String])]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [ValidateNotNullOrEmpty()]
        [string] $BaseUrl
    )
    $FullUrl = '{0}/api/File' -f $BaseUrl
    Write-Verbose "Upload Url: $FullUrl"
    return $FullUrl
}