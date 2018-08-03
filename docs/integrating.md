# Integrating

## Collecting the MSBuild Binary Log file

In order to capture a binary log file from MSBuild your project must be built with MSBuild 15.3 (Visual Studio 2017 Update 3) or greater. The switch `/bl` must be added to your MSBuild command.

`msbuild MySolution.sln /bl` will output the binary log file to `msbuild.binlog`.

`msbuild MySolution.sln /bl:output.binlog` will output the binary log file to `output.binlog`.

MSBuild will error if the specified file path does not end with `binlog`.

More information about the MSBuild Binary Log format can be found [here](http://msbuildlog.com/).

## Integrating on AppVeyor

1. Copy `MSBuildLogOctokitChecker.psd1` and `MSBuildLogOctokitChecker.psm1` from MSBLOC.Posh to your project
   - [MSBuildLogOctokitChecker.psd1](../MSBLOC.Post/MSBuildLogOctokitChecker.psd1)
   - [MSBuildLogOctokitChecker.psm1](../MSBLOC.Post/MSBuildLogOctokitChecker.psm1)
1. Modify you build script to perform the following steps

   ```
   version: 1.0.{build}
   image: Visual Studio 2017
   build_script:
      - ps: >-
       Import-Module .\tools\MSBLOC.Posh\MSBuildLogOctokitChecker.psm1
       msbuild TestConsoleApp1.sln --% /bl:output.binlog verbosity=diagnostic
    
       if (! $?) {
         echo "Build Error"
         Send-MsbuildLogAppveyor output.binlog
         exit -1
       }
    
       Send-MsbuildLogAppveyor $binLogPath
   ```
   The key points are as follows

   1. Using Powershell
   1. Importing the MSBLOC.Posh Powershell Module
   1. Invoking MSBuild with the option to output the binary log
   1. Captuing errors from MSBuild
      - Invoking `Send-MsbuildLogAppveyor` to send MSBLOC the binary log file
      - Returning an error for MSBuild
   1. Invoking `Send-MsbuildLogAppveyor` to send MSBLOC the binary log file

   An example can be found [here](https://github.com/justaprogrammer/TestConsoleApp1/blob/appveyor/appveyor.yml)

## Integrating with other CI systems

To facilitate integration on another CI system there is a Powershell command from the same module named `Send-MsbuildLog`. This command should be provided the follow parameters.
- `-Path` - Location to the binary log file
- `-RepoOwner` - Owner of the GitHub Repository
- `-RepoName` - Name of the GitHub Repository
- `-CloneRoot` - Directory where build occured
- `-HeadCommit` - Sha of the commit for the build log file