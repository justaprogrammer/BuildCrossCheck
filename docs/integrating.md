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
       msbuild TestConsoleApp1.sln --% /bl:output.binlog /flp:logfile=output.log;verbosity=diagnostic
    
       if (! $?) {
         echo "Build Error"
         Push-AppveyorArtifact output.log -Type log
         Push-AppveyorArtifact output.binlog -Type log
         Send-MsbuildLogAppveyor output.binlog
         exit -1
       }
       $binLogPath = (Get-Item -Path ".\").FullName + "\output.binlog"
    
       Send-MsbuildLogAppveyor $binLogPath
   ```

## Integrating on Other CI
