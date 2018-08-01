
$script:BaseUrl = 'http://msblocweb.azurewebsites.net'

function  Send-MsbuildLog {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [ValidateScript({ [System.IO.File]::Exists($_)})]
        [ValidateNotNullOrEmpty()]
        [string] $Path,
        [ValidateNotNullOrEmpty()]
        [string] $RepoOwner,
        [ValidateNotNullOrEmpty()]
        [string] $RepoName,
        [ValidateNotNullOrEmpty()]
        [string] $CloneRoot,
        [ValidateNotNullOrEmpty()]
        [Alias('Sha', 'CommitHash')]
        [string] $HeadCommit
    )

    #TODO: Stream this
    $FileBytes = [System.IO.File]::ReadAllBytes($Path);
    $FileEnc = [System.Text.Encoding]::GetEncoding('UTF-8').GetString($FileBytes);
    $FileInfo = New-Object System.IO.FileInfo @($Path)
    $Boundary = [System.Guid]::NewGuid().ToString();
    $LF = "`r`n";
    $Body = @(
        "--$Boundary",
        "Content-Disposition: form-data; name=`"BinaryLogFile`"; filename=`"$($FileInfo.Name)`"",
        "Content-Type: application/octet-stream$LF",
        $FileEnc,
        "--$Boundary",
        "Content-Disposition: form-data; name=`"RepoOwner`"$LF",
        $RepoOwner
        "--$Boundary",
        "Content-Disposition: form-data; name=`"RepoName`"$LF",
        $RepoName
        "--$Boundary",
        "Content-Disposition: form-data; name=`"CloneRoot`"$LF",
        $CloneRoot
        "--$Boundary",
        "Content-Disposition: form-data; name=`"CommitSha`"$LF",
        $HeadCommit
        "--$Boundary--$LF"
    ) -join $LF

    $Uri = GetUploadUrl $script:BaseUrl
    Invoke-RestMethod `
        -Method POST `
        -Uri $Uri `
        -ContentType "multipart/form-data; boundary=`"$Boundary`"" `
        -Body $Body

}

function  Send-MsbuildLogAppveyor {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [ValidateScript({ [System.IO.File]::Exists($_)})]
        [ValidateNotNullOrEmpty()]
        [string] $Path
    )

    $RepoOwner, $RepoName = $env:APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME.Split('/');
    $CloneRoot = $env:APPVEYOR_BUILD_FOLDER
    $HeadCommit = $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT

    Send-MsbuildLog $Path $CloneRoot $RepoOwner $RepoName $HeadCommit
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
    $FullUrl = '{0}/api/log/upload' -f $BaseUrl
    Write-Verbose "Upload Url: $FullUrl"
    return $FullUrl
}
