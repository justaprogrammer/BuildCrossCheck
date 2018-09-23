# Integrating

## Adding the GitHub App to your Organization or Repository

Add the [BCC GitHub App](https://github.com/apps/msbuildlog-octokit-checker) to the repositories that you 
would like to work with.

## Create a token

In order to verify your build server, create a token to use when sending build logs.
After installing the application to your repo, head to [BCC WebApp](https://msblocweb.azurewebsites.net/) 
and login with your GitHub account. There you will be able create a token per repository as well as
revoke prior tokens if necessary.

## Collecting the MSBuild Binary Log file

In order to capture a binary log file from MSBuild your project must be built with MSBuild 15.3 (Visual Studio 
2017 Update 3) or greater. The switch `/bl` must be added to your MSBuild command.

`msbuild MySolution.sln /bl` will output the binary log file to `msbuild.binlog`.

`msbuild MySolution.sln /bl:output.binlog` will output the binary log file to `output.binlog`.

MSBuild will error if the specified file path does not end with `binlog`.

More information about the MSBuild Binary Log format can be found [here](http://msbuildlog.com/).

**_WARNING:_** The `binlog` format captures all environment variables present at build time.
This means that any build time secrets kept in environment variables will be recorded in the file.
Please review your build environment variables as well as your own `binlog` output to make sure
there are no secrets you would not feel comfortable with releasing.
We do not keep a copy of you binary log file.

## Integrating on AppVeyor

1. Copy `MSBuildLogOctokitChecker.psd1` and `MSBuildLogOctokitChecker.psm1` from MSBLOC.Posh to your project
   - [MSBuildLogOctokitChecker.psd1](../MSBLOC.Posh/MSBuildLogOctokitChecker.psd1)
   - [MSBuildLogOctokitChecker.psm1](../MSBLOC.Posh/MSBuildLogOctokitChecker.psm1)
1. Modify you build script to perform the following steps

   ```
   version: 1.0.{build}
   image: Visual Studio 2017
   environment:
     MSBLOC_JWT:
       secure: [Encrypted Token]
   build_script:
      - ps: >-
          msbuild TestConsoleApp1.sln --% /bl:output.binlog
   on_finish:
      - ps: >-
          if(-not $env:APPVEYOR_PULL_REQUEST_NUMBER)
          {
              Import-Module .\tools\MSBLOC.Posh\MSBuildLogOctokitChecker.psm1
              Send-MsbuildLogAppveyor -Path output.binlog -Token $env:MSBLOC_JWT
          }
   ```
   The key points are as follows

   1. Invoking MSBuild with the option to output the binary log
   1. Using the `on_finish` task with a PowerShell script
      1. If it is a branch build
         1. Import the MSBLOC.Posh Powershell Module and send MSBLOC the binary log file

   An example can be found [here](https://github.com/justaprogrammer/TestConsoleApp1/blob/appveyor/appveyor.yml)

## Integrating with other CI systems

To facilitate integration on another CI system there is a Powershell command from the same module named 
`Send-MsbuildLog`. This command should be provided the follow parameters.
- `-Path` - Location to the binary log file
- `-Token` - The token created for the repository.
- `-CloneRoot` - Directory where build occured
- `-HeadCommit` - Sha of the commit for the build log file
