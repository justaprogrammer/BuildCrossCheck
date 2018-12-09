# Integrating

## Adding the GitHub App

Add the **Build Cross Check** [GitHub App](https://github.com/apps/build-cross-check) to the repositories that you 
would like to work with.

## Create a token

In order to verify your build server, create a token to use when sending build logs.
After installing the application to your repo, head to **Build Cross Check** [web app](https://buildcrosscheck.azurewebsites.net/) 
and login with your GitHub account. There you will be able create a token per repository as well as
revoke prior tokens if necessary.

## Build Integration

### MSBuild Warnings and Errors

This example is based on AppVeyor, but this will work from any CI system. For more information see the documentation for [BCC-MSBuildLog](https://github.com/justaprogrammer/BCC-MSBuildLog/blob/master/docs/usage.md) and [BCC-Submssion](https://github.com/justaprogrammer/BCC-Submission/blob/master/docs/usage.md).

1. Install the GitHub App on the desired GitHub Repositories or Organizations.

1. Generate a token using the steps [above](#create-a-token) and set a [secure environment variable](https://www.appveyor.com/docs/build-configuration/#secure-variables) named `BCC_TOKEN`.

1. Add an install step to `appveyor.yml` to download **BCC-MSBuildLog** and **BCC-Submission** from Chocolately

   ```yml
   os: Visual Studio 2017
   install:
   - choco install --no-progress BCC-MSBuildLog
   - choco install --no-progress BCC-Submission
   ```

1. Configure your build to output a MSBuild binary log file

   ```yml
   build_script:
   - msbuild TestConsoleApp1.sln /bl:output.binlog
   ```

1. Use **BCC-MSBuildLog** and **BCC-Submission** to create and submit check run data to **Build Cross Check**.

   ```yml
   on_finish:
   - IF NOT "%BCC_TOKEN%x"=="x" BCCMSBuildLog --input output.binlog --output checkrun.json --cloneRoot "%APPVEYOR_BUILD_FOLDER%" --ownerRepo %APPVEYOR_REPO_NAME% --hash %APPVEYOR_REPO_COMMIT%
   - IF NOT "%BCC_TOKEN%x"=="x" BCCSubmission -i checkrun.json -h %APPVEYOR_REPO_COMMIT% -t %BCC_TOKEN%
   ```

1. Enjoy your warnings and errors.

### Custom Check Run Data

### More coming soon...